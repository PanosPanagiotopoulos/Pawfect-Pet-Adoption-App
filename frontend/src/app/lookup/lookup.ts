export interface Lookup {
  offset: number;
  pageSize: number;
  query?: string;
  fields: string[];
  sortBy: string[];
  sortDescending?: boolean;
}
