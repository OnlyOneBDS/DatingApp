import { Photo } from "./photo";

export interface Member {
  id: number;
  userName: string;
  age: number;
  gender: string;
  knownAs: string;
  introduction: string;
  lookingFor: string;
  interests: string;
  city: string;
  country: string;
  photoUrl: string;
  photos: Photo[];
  created: Date;
  lastActive: Date;
}