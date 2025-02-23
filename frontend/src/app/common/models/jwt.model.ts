export interface JwtPayload {
  nameid: string;
  email: string;
  role: string;
  exp: number;
  iat: number;
}
