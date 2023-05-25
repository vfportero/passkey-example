export class StorageService {
  userIdKey = 'userId';

  getAuthenticatedUserId = () => {
    return sessionStorage.getItem(this.userIdKey);
  };

  setAuthenticatedUserId = (userId: string) => {
    sessionStorage.setItem(this.userIdKey, userId);
  };

  removeAuthenticatedUserId = () => {
    sessionStorage.removeItem(this.userIdKey);
  };
}
