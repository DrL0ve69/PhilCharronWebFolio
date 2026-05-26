export interface AuthResponseDto {
  token: string;
  userId: string;
  email: string;
  role: string;
  expiration: string;
}

export interface ProfileDto {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
}
