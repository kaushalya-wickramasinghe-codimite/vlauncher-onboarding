export interface UserIdentity {
  email: string;
  id: string;
}

export class IdentityService {
  async getUserEmail(): Promise<UserIdentity | null> {
    return new Promise((resolve) => {
      chrome.identity.getProfileUserInfo({ accountStatus: 'ANY' }, (userInfo) => {
        if (chrome.runtime.lastError) {
          console.error('Error getting user info:', chrome.runtime.lastError.message);
          resolve(null);
          return;
        }

        if (userInfo && userInfo.email) {
          resolve({
            email: userInfo.email,
            id: userInfo.id,
          });
        } else {
          resolve(null);
        }
      });
    });
  }

  async getAuthToken(interactive: boolean = false): Promise<string | null> {
    return new Promise((resolve) => {
      chrome.identity.getAuthToken({ interactive }, (result) => {
        if (chrome.runtime.lastError) {
          console.error('Error getting auth token:', chrome.runtime.lastError.message);
          resolve(null);
          return;
        }

        // Handle both old and new API response format
        const token = typeof result === 'string' ? result : result?.token;
        resolve(token || null);
      });
    });
  }
}

export const identityService = new IdentityService();
