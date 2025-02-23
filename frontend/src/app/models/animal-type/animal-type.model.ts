// AnimalType Models
export interface AnimalType {
  Id?: string;
  Name?: string;
  Description?: string;
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface AnimalTypePersist {
  Id: string;
  Name: string;
  Description: string;
}
