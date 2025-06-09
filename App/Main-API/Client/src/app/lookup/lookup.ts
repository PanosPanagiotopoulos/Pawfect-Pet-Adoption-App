export interface Lookup {
  offset: number;
  pageSize: number;
  query?: string;
  excludedIds?: string[];
  fields: string[];
  sortBy: string[];
  sortDescending?: boolean;
}
