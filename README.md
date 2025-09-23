# Pawfect - Εφαρμογή Υιοθεσίας Ζώων

Το **Pawfect** είναι μια σύγχρονη πλατφόρμα υιοθεσίας ζώων που αναπτύχθηκε με στόχο να συνδέει τους χρήστες με καταφύγια και να καθιστά τη διαδικασία αναζήτησης και υιοθεσίας κατοικιδίων πιο **εύκολη, έξυπνη και ασφαλή**.  

Η αρχιτεκτονική του συστήματος βασίζεται σε **μικροϋπηρεσίες** και περιλαμβάνει:

- **.NET και C# (Backend)**: Παρέχουν μια στιβαρή και επεκτάσιμη βάση για REST APIs, authentication/authorization με JWT, worker services για ειδοποιήσεις και ασφαλή middleware pipelines.
- **Angular (Frontend)**: Δυναμική, responsive SPA διεπαφή με TypeScript, RxJS, Reactive Forms και Material UI για μοντέρνο, φιλικό προς τον χρήστη σχεδιασμό.
- **MongoDB Atlas (Database)**: NoSQL document database για ευέλικτη αποθήκευση δεδομένων (χρήστες, καταφύγια, ζώα, αιτήσεις υιοθεσίας, μηνύματα).
- **SignalR (Real-time)**: Εξασφαλίζει άμεση επικοινωνία και ειδοποιήσεις σε πραγματικό χρόνο.
- **GraphQL-Inspired Query Layer**: Επιτρέπει client-driven ερωτήματα με φίλτρα, sorting και pagination, μειώνοντας το over-fetching.
- **Authentication & Authorization**: JWT tokens, role-based access (User, Shelter, Admin), με συμμόρφωση σε OWASP standards.
- **SendGrid (Emails) & Vonage (SMS)**: Για αξιόπιστες ειδοποιήσεις και επικοινωνία με χρήστες.
- **Mistral AI (AI Recommendations)**: Παρέχει έξυπνες προτάσεις υιοθεσίας και conversational guidance με RAG (Retrieval-Augmented Generation).
- **AWS S3 (Storage)**: Για ασφαλή και κλιμακώσιμη αποθήκευση αρχείων και εικόνων.
- **Docker (Deployment)**: Containerization των υπηρεσιών για φορητότητα, συνέπεια και εύκολη ανάπτυξη.

Η επιλογή αυτών των τεχνολογιών διασφαλίζει ότι η πλατφόρμα είναι **ασφαλής, επεκτάσιμη, cloud-ready** και προσαρμοσμένη στις μελλοντικές ανάγκες.

---

## Περιεχόμενα

- [Χαρακτηριστικά](#χαρακτηριστικά)
- [Αρχιτεκτονική](#αρχιτεκτονική)
- [Τεχνολογίες](#τεχνολογίες)
- [Εγκατάσταση με Docker](#εγκατάσταση-με-docker)
---

## Χαρακτηριστικά

- **Πιστοποίηση Χρήστη και Διαχείριση Δικαιωμάτων Πρόσβασης**
- **Διαχείριση Προφίλ** (χρήστες, καταφύγια, ζώα)
- **Σύνθετη Αναζήτηση και Chatbot Προτάσεων**
- **Σύστημα Μηνυμάτων σε Πραγματικό Χρόνο**
- **Ειδοποιήσεις μέσω Email και SMS**
- **Πίνακας Διαχείρισης για Admins**

---

## Αρχιτεκτονική

Η εφαρμογή ακολουθεί αρχιτεκτονική **microservices**, όπου κάθε υπηρεσία (API, Notifications, Messenger) εκτελείται ανεξάρτητα μέσα σε **Docker containers**.  

- **Backend**: ASP.NET Core Web API, SignalR, Worker Services  
- **Frontend**: Angular SPA  
- **Database**: MongoDB Atlas  
- **Messaging**: WebSockets μέσω SignalR  
- **Storage**: AWS S3  

---

## Τεχνολογίες

- **Backend**: .NET, ASP.NET Core, C#  
- **Frontend**: Angular, RxJS, Angular Router  
- **Database**: MongoDB Atlas  
- **Auth**: JWT, OAuth2, Role-based access  
- **Notifications**: SendGrid (Email), Vonage (SMS)  
- **AI**: Mistral AI (RAG-based recommendations)  
- **Storage**: AWS S3  
- **Deployment**: Docker, Docker Compose  

---

## Εγκατάσταση με Docker

Η εφαρμογή χρησιμοποιεί **Docker Compose** για να εκκινεί όλα τα services.

### Προαπαιτούμενα
- Docker Desktop ή Docker Engine  
- Docker Compose v2+  
- Git  

### Βήματα Εγκατάστασης

1. **Clone του Repository**
   ```bash
   git clone https://github.com/PanosPanagiotopoulos/Pawfect-Pet-Adoption-App.git Pawfect
   cd Pawfect/App
   ```

2. **Απόκτηση αρχείων secrets**
   Ζητήστε να λάβετε το αρχείο `secrets.zip` εδώ: https://drive.google.com/file/d/1p1bCmD_b7C4JTp1PhbUPQVG3k5lbjT_I/view?usp=drive_link
   Εξάγετε το **μέσα στο `/App`** ώστε να έχετε:

   ```
   /App/secrets/
     pawfect-api.environment.json
     pawfect-notifications.environment.json
     pawfect-messenger.environment.json

   /App/frontend_secrets/
     environment.Production.ts
     environment.Development.ts
   ```

3. **Αντιγραφή του Angular environment πριν το build**
   ```bash
   # Linux/macOS
   cp ./frontend_secrets/environment.Production.ts ./Pawfect-API/Client/src/environments/environment.Production.ts
   cp ./frontend_secrets/environment.Development.ts ./Pawfect-API/Client/src/environments/environment.Development.ts
   ```

   ```powershell
   # Windows PowerShell
   Copy-Item .\frontend_secrets\environment.Production.ts .\Pawfect-API\Client\src\environments\environment.Production.ts -Force
   Copy-Item .\frontend_secrets\environment.Development.ts .\Pawfect-API\Client\src\environments\environment.Development.ts -Force
   ```

4. **Build και εκκίνηση containers**
   ```bash
   docker compose build --parallel
   docker compose up
   ή
   docker compose up --build
   ```

5. **Πρόσβαση στις υπηρεσίες**
   - Main API & Angular frontend → [http://localhost:5000](http://localhost:5000)  
   - Notifications API → [http://localhost:5001](http://localhost:5001)  
   - Messenger API → [http://localhost:5002](http://localhost:5002)  
