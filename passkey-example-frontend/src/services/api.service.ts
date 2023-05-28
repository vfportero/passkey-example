export class ApiService {
  apiUrl = process.env.VUE_APP_BASE_API_URL ?? 'https://localhost:8081';

  createUser = async (email: string) => {
    const response = await fetch(`${this.apiUrl}/users`, {
      method: 'POST',
      body: JSON.stringify({
        email: email,
      }),
      headers: {
        'Content-type': 'application/json; charset=UTF-8',
      },
    });
    return response.json();
  };

  makeCredentialOptions = async (email: string) => {
    const response = await fetch(`${this.apiUrl}/makeCredentialOptions`, {
      method: 'POST',
      body: JSON.stringify({
        userName: email,
        displayName: email,
        attType: 'none',
        authType: 'platform',
        userVerification: 'required',
      }),
      headers: {
        'Content-type': 'application/json; charset=UTF-8',
      },
    });

    return response.json();
  };

  addUserCredential = async (attestationResponse: any, credentialOptions: string) => {
    const response = await fetch(`${this.apiUrl}/users/addCredential`, {
      method: 'POST',
      body: JSON.stringify({
        attestationResponse,
        credentialOptions,
      }),
      headers: {
        'Content-type': 'application/json; charset=UTF-8',
      },
    });

    return response.json();
  };

  getUser = async (id: string): Promise<{ id: string; email: string }> => {
    const response = await fetch(`${this.apiUrl}/users/${id}`);
    return response.json();
  };

  getUserCredentials = async (id: string) => {
    const response = await fetch(`${this.apiUrl}/users/${id}/credentials`);
    return response.json();
  };

  queryUser = async (email: string): Promise<Array<any>> => {
    const response = await fetch(`${this.apiUrl}/users?q=${email}`);
    return response.json();
  };

  makeAssertionOptions = async (email: string) => {
    const response = await fetch(`${this.apiUrl}/makeAssertionOptions`, {
      method: 'POST',
      body: JSON.stringify({
        UserName: email,
      }),
      headers: {
        'Content-type': 'application/json; charset=UTF-8',
      },
    });

    return response.json();
  };
}
