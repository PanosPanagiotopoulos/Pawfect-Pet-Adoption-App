export interface CountryCode {
  code: string;
  name: string;
  flag: string;
}

export const COUNTRY_CODES: CountryCode[] = [
  { code: '+30', name: 'Greece', flag: '🇬🇷' },
  { code: '+31', name: 'Netherlands', flag: '🇳🇱' },
  { code: '+32', name: 'Belgium', flag: '🇧🇪' },
  { code: '+33', name: 'France', flag: '🇫🇷' },
  { code: '+34', name: 'Spain', flag: '🇪🇸' },
  { code: '+350', name: 'Gibraltar', flag: '🇬🇮' },
  { code: '+351', name: 'Portugal', flag: '🇵🇹' },
  { code: '+352', name: 'Luxembourg', flag: '🇱🇺' },
  { code: '+353', name: 'Ireland', flag: '🇮🇪' },
  { code: '+354', name: 'Iceland', flag: '🇮🇸' },
  { code: '+355', name: 'Albania', flag: '🇦🇱' },
  { code: '+356', name: 'Malta', flag: '🇲🇹' },
  { code: '+357', name: 'Cyprus', flag: '🇨🇾' },
  { code: '+358', name: 'Finland', flag: '🇫🇮' },
  { code: '+359', name: 'Bulgaria', flag: '🇧🇬' },
  { code: '+36', name: 'Hungary', flag: '🇭🇺' },
  { code: '+370', name: 'Lithuania', flag: '🇱🇹' },
  { code: '+371', name: 'Latvia', flag: '🇱🇻' },
  { code: '+372', name: 'Estonia', flag: '🇪🇪' },
  { code: '+373', name: 'Moldova', flag: '🇲🇩' },
  { code: '+374', name: 'Armenia', flag: '🇦🇲' },
  { code: '+375', name: 'Belarus', flag: '🇧🇾' },
  { code: '+376', name: 'Andorra', flag: '🇦🇩' },
  { code: '+377', name: 'Monaco', flag: '🇲🇨' },
  { code: '+378', name: 'San Marino', flag: '🇸🇲' },
  { code: '+380', name: 'Ukraine', flag: '🇺🇦' },
  { code: '+381', name: 'Serbia', flag: '🇷🇸' },
  { code: '+382', name: 'Montenegro', flag: '🇲🇪' },
  { code: '+385', name: 'Croatia', flag: '🇭🇷' },
  { code: '+386', name: 'Slovenia', flag: '🇸🇮' },
  { code: '+387', name: 'Bosnia and Herzegovina', flag: '🇧🇦' },
  { code: '+389', name: 'North Macedonia', flag: '🇲🇰' },
  { code: '+39', name: 'Italy', flag: '🇮🇹' },
  { code: '+40', name: 'Romania', flag: '🇷🇴' },
  { code: '+41', name: 'Switzerland', flag: '🇨🇭' },
  { code: '+420', name: 'Czech Republic', flag: '🇨🇿' }
].sort((a, b) => a.name.localeCompare(b.name));