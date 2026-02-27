export interface AuthResponse { tokenType?: string | null; accessToken: string; refreshToken: string; expiresIn: number; }
export interface InfoRequest { newEmail?: string | null; newPassword?: string | null; oldPassword?: string | null; }
export interface InfoResponse { email: string; isEmailConfirmed: boolean; }

export interface HttpValidationProblemDetails {
  type?: string | null;
  title?: string | null;
  status?: number | string | null;
  detail?: string | null;
  instance?: string | null;
  errors?: { [key: string]: string[]; };
}

export interface TwoFactorRequest {
  enable?: boolean | null;
  twoFactorCode?: string | null;
  resetSharedKey?: boolean;
  resetRecoveryCodes?: boolean;
  forgetMachine?: boolean;
}

export interface TwoFactorResponse {
  sharedKey: string;
  recoveryCodesLeft: number;
  recoveryCodes?: string[] | null;
  isTwoFactorEnabled: boolean;
  isMachineRemembered: boolean;
}