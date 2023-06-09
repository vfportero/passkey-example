import { ApiService } from './api.service';

export class PasskeyService {
  coerceToArrayBuffer = function (thing: any) {
    if (typeof thing === 'string') {
      // base64url to base64
      thing = thing.replace(/-/g, '+').replace(/_/g, '/');

      // base64 to Uint8Array
      const str = window.atob(thing);
      const bytes = new Uint8Array(str.length);
      for (let i = 0; i < str.length; i++) {
        bytes[i] = str.charCodeAt(i);
      }
      thing = bytes;
    }

    // Array to Uint8Array
    if (Array.isArray(thing)) {
      thing = new Uint8Array(thing);
    }

    // Uint8Array to ArrayBuffer
    if (thing instanceof Uint8Array) {
      thing = thing.buffer;
    }

    // error if none of the above worked
    if (!(thing instanceof ArrayBuffer)) {
      throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
    }

    return thing;
  };

  coerceToBase64Url = function (thing: any) {
    // Array or ArrayBuffer to Uint8Array
    if (Array.isArray(thing)) {
      thing = Uint8Array.from(thing);
    }

    if (thing instanceof ArrayBuffer) {
      thing = new Uint8Array(thing);
    }

    // Uint8Array to base64
    if (thing instanceof Uint8Array) {
      let str = '';
      const len = thing.byteLength;

      for (let i = 0; i < len; i++) {
        str += String.fromCharCode(thing[i]);
      }
      thing = window.btoa(str);
    }

    if (typeof thing !== 'string') {
      throw new Error('could not coerce to string');
    }

    // base64 to base64url
    // NOTE: "=" at the end of challenge is optional, strip it off here
    thing = thing.replace(/\+/g, '-').replace(/\//g, '_').replace(/=*$/g, '');

    return thing;
  };

  browserHasPasskeyFeature = async () => {
    if (window.PublicKeyCredential) {
      const isUVSupported = (await (PublicKeyCredential as any).isUserVerifyingPlatformAuthenticatorAvailable?.()) ?? false;

      return (await this.browserHasWebAuthnSupport()) && isUVSupported;
    }
    return false;
  };

  browserHasWebAuthnSupport = async () => {
    if (window.PublicKeyCredential) {
      const isCMSupported = (await (PublicKeyCredential as any).isConditionalMediationAvailable?.()) ?? false;

      return isCMSupported;
    }
    return false;
  };

  createPasskey = async (userEmail: string) => {
    const apiService = new ApiService();
    const creadentialOptions = await apiService.makeCredentialOptions(userEmail);

    const credentialOptionsJson = JSON.stringify(creadentialOptions);

    creadentialOptions.challenge = this.coerceToArrayBuffer(creadentialOptions.challenge);
    creadentialOptions.user.id = this.coerceToArrayBuffer(creadentialOptions.user.id);
    creadentialOptions.excludeCredentials = creadentialOptions.excludeCredentials.map((c: any) => {
      c.id = this.coerceToArrayBuffer(c.id);
    });

    const credential = (await navigator.credentials.create({
      publicKey: creadentialOptions,
    })) as any;

    const attestationObject = new Uint8Array(credential?.response?.attestationObject);
    const clientDataJSON = new Uint8Array(credential?.response?.clientDataJSON);
    const rawId = new Uint8Array(credential?.rawId);

    const makeCredentialRequest = {
      id: credential?.id,
      rawId: this.coerceToBase64Url(rawId),
      type: credential?.type,
      extensions: credential?.getClientExtensionResults(),
      response: {
        AttestationObject: this.coerceToBase64Url(attestationObject),
        clientDataJSON: this.coerceToBase64Url(clientDataJSON),
      },
    };

    return await apiService.addUserCredential(makeCredentialRequest, credentialOptionsJson);
  };

  validatePasskey = async (userEmail: string) => {
    const apiService = new ApiService();
    const makeAssertionOptions = await apiService.makeAssertionOptions(userEmail);

    const makeAssertionOptionsJson = JSON.stringify(makeAssertionOptions);

    const challenge = this.coerceToBase64Url(makeAssertionOptions.challenge);
    makeAssertionOptions.challenge = this.coerceToArrayBuffer(challenge);

    // fix escaping. Change this to coerce
    makeAssertionOptions.allowCredentials.forEach((listItem: any) => {
      const fixedId = this.coerceToBase64Url(listItem.id);
      listItem.id = this.coerceToArrayBuffer(fixedId);
    });

    // ask browser for credentials (browser will ask connected authenticators)
    let credential;
    try {
      credential = (await navigator.credentials.get({ publicKey: makeAssertionOptions })) as any;
    } catch (err: any) {
      console.error(err.message ? err.message : err);
    }

    try {
      // Move data into Arrays incase it is super long
      const authData = new Uint8Array(credential.response.authenticatorData);
      const clientDataJSON = new Uint8Array(credential.response.clientDataJSON);
      const rawId = new Uint8Array(credential.rawId);
      const sig = new Uint8Array(credential.response.signature);
      const data = {
        id: credential.id,
        rawId: this.coerceToBase64Url(rawId),
        type: credential.type,
        extensions: credential.getClientExtensionResults(),
        response: {
          authenticatorData: this.coerceToBase64Url(authData),
          clientDataJSON: this.coerceToBase64Url(clientDataJSON),
          signature: this.coerceToBase64Url(sig),
        },
      };

      const response = await apiService.makeAssertion(data, makeAssertionOptionsJson);

      // show error
      if (response.status !== 'ok') {
        console.log('Error doing assertion');
        console.log(response.errorMessage);
        console.error(response.errorMessage);
      }

      return response;
    } catch (e) {
      console.error('Could not verify assertion', e);
    }

    return null;
  };
}
