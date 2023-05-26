import { ApiService } from './api.service';

export class PasskeyService {
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

    creadentialOptions.challenge = Uint8Array.from(atob(creadentialOptions.challenge), (c) => c.charCodeAt(0));
    creadentialOptions.user.id = Uint8Array.from(atob(creadentialOptions.user.id), (c) => c.charCodeAt(0));
    creadentialOptions.excludeCredentials = creadentialOptions.excludeCredentials.map((c: any) => {
      c.id = Uint8Array.from(atob(c.id), (c) => c.charCodeAt(0));
      return c;
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
