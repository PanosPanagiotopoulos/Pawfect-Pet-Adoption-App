// AnimalType Models
export interface AnimalType {
  id?: string;
  name?: string;
  description?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface AnimalTypePersist {
  id: string;
  name: string;
  description: string;
}
