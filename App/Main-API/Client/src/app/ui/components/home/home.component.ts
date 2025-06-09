import { Component } from '@angular/core';
interface Feature {
  icon: string;
  title: string;
  description: string;
  bgColor: string;
  iconColor: string;
  gradientClass: string;
}

@Component({
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  standalone: false,
})
export class HomeComponent {
  currentYear = new Date().getFullYear();

  features: Feature[] = [
    {
      icon: 'lucideSearch',
      title: 'Αναζήτηση Κατοικιδίων',
      description:
        'Βρείτε τον τέλειο σύντροφο ανάμεσα στην προσεκτικά επιλεγμένη συλλογή αγαπημένων κατοικιδίων μας',
      bgColor: 'bg-gradient-to-br from-primary-500/20 to-primary-400/20',
      iconColor: 'text-primary-400',
      gradientClass: 'feature-card-primary',
    },
    {
      icon: 'lucideHeart',
      title: 'Αντιστοίχιση & Σύνδεση',
      description:
        'Το έξυπνο σύστημα αντιστοίχισής μας σας βοηθά να βρείτε και να επικοινωνήσετε με το αρμόδιο καταφύγιο για να υιοθετήσετε το κατοικίδιο για τον τρόπο ζωής σας',
      bgColor: 'bg-gradient-to-br from-secondary-500/20 to-secondary-400/20',
      iconColor: 'text-secondary-400',
      gradientClass: 'feature-card-secondary',
    },
    {
      icon: 'lucideMessageCircle',
      title: 'Υιοθεσία & Αγάπη',
      description:
        'Ξεκινήστε το ταξίδι της αγάπης και της συντροφικότητας με το νέο σας τετράποδο φίλο',
      bgColor: 'bg-gradient-to-br from-accent-500/20 to-accent-400/20',
      iconColor: 'text-accent-400',
      gradientClass: 'feature-card-accent',
    },
  ];
}