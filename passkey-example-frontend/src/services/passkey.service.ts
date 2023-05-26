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

  browserHasPasskeyFeature = async () => {
    if (window.PublicKeyCredential) {
      const isCMSupported = (await (PublicKeyCredential as any).isConditionalMediationAvailable?.()) ?? false;
      const isUVSupported = (await (PublicKeyCredential as any).isUserVerifyingPlatformAuthenticatorAvailable?.()) ?? false;

      return isCMSupported && isUVSupported;
    }
    return false;
  };

  createPasskey = async (userEmail: string) => {
    const apiService = new ApiService();
    const creadentialOptions = await apiService.makeCredentialOptions(userEmail);

    creadentialOptions.challenge = this.coerceToArrayBuffer(creadentialOptions.challenge);
    creadentialOptions.user.id = this.coerceToArrayBuffer(creadentialOptions.user.id);
    creadentialOptions.excludeCredentials = creadentialOptions.excludeCredentials.map((c: any) => {
      c.id = this.coerceToArrayBuffer(c.id);
    });

    // const publicKeyCredentialCreationOptions: PublicKeyCredentialCreationOptions = {
    //   challenge: new Uint8Array([1, 2, 3, 4, 5, 6, 7, 8]).buffer,
    //   rp: {
    //     name: 'Example',
    //     id: process.env.VUE_APP_DOMAIN ?? 'localhost',
    //   },
    //   user: {
    //     id: new TextEncoder().encode(userId),
    //     name: userEmail,
    //     displayName: userEmail,
    //   },
    //   pubKeyCredParams: [
    //     { alg: -7, type: 'public-key' },
    //     { alg: -257, type: 'public-key' },
    //   ],
    //   // excludeCredentials: [{
    //   //   id: *****,
    //   //   type: 'public-key',
    //   //   transports: ['internal'],
    //   // }],
    //   authenticatorSelection: {
    //     authenticatorAttachment: 'platform',
    //     requireResidentKey: true,
    //   },
    // };

    const credential = await navigator.credentials.create({
      publicKey: creadentialOptions,
    });

    console.log(credential);
  };
}
