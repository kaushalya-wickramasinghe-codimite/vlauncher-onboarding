declare namespace chrome {
  namespace identity {
    interface TokenDetails {
      interactive?: boolean;
      account?: { id: string };
      scopes?: string[];
    }

    interface UserInfo {
      email: string;
      id: string;
    }

    function getAuthToken(
      details: TokenDetails,
      callback: (token?: string) => void
    ): void;

    function getProfileUserInfo(
      details: { accountStatus?: 'ANY' | 'SYNC' },
      callback: (userInfo: UserInfo) => void
    ): void;

    function removeCachedAuthToken(
      details: { token: string },
      callback?: () => void
    ): void;
  }

  namespace runtime {
    const lastError: { message?: string } | undefined;
  }
}
