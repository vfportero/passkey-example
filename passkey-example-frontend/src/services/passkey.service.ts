export class PasskeyService {
  browserHasPasskeyFeature = async () => {
    // Availability of `window.PublicKeyCredential` means WebAuthn is usable.
    // `isUserVerifyingPlatformAuthenticatorAvailable` means the feature detection is usable.
    // `isConditionalMediationAvailable` means the feature detection is usable.

    if (window.PublicKeyCredential) {
      // Check if user verifying platform authenticator is available.

      const isCMSupported = (await (PublicKeyCredential as any).isConditionalMediationAvailable?.()) ?? false;
      const isUVSupported = (await (PublicKeyCredential as any).isUserVerifyingPlatformAuthenticatorAvailable?.()) ?? false;

      return isCMSupported && isUVSupported;
    }
    return false;
  };

  createPasskey = async (userId: string, userEmail: string) => {
    const publicKeyCredentialCreationOptions: PublicKeyCredentialCreationOptions = {
      challenge: new Uint8Array([1, 2, 3, 4, 5, 6, 7, 8]).buffer,
      rp: {
        name: 'Example',
        id: process.env.VUE_APP_DOMAIN ?? 'localhost',
      },
      user: {
        id: new TextEncoder().encode(userId),
        name: userEmail,
        displayName: userEmail,
      },
      pubKeyCredParams: [
        { alg: -7, type: 'public-key' },
        { alg: -257, type: 'public-key' },
      ],
      // excludeCredentials: [{
      //   id: *****,
      //   type: 'public-key',
      //   transports: ['internal'],
      // }],
      authenticatorSelection: {
        authenticatorAttachment: 'platform',
        requireResidentKey: true,
      },
    };

    const credential = await navigator.credentials.create({
      publicKey: publicKeyCredentialCreationOptions,
    });
  };
}
