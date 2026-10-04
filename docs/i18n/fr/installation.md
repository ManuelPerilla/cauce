# Installation de Cauce

La préversion 0.6.0-rc.1 est distribuée en ZIP portable pour Windows x64 ou ARM64. Elle est **non signée** tant que les conditions externes de signature ne sont pas remplies. Choisissez l’architecture de votre ordinateur.

1. Téléchargez le ZIP d’une publication officielle et vérifiez son SHA-256.
2. Extrayez-le dans un dossier accessible en écriture.
3. Lancez `Cauce.exe` ; le runtime .NET est inclus.
4. Dans **Biblioteca → Añadir archivos**, sélectionnez des MP3, WAV ou M4A. Complétez le genre et démarrez une session dans **Escuchar**.

Aucun compte ni droit administrateur n’est nécessaire. Le programme ne modifie pas PATH. Les données sont conservées dans `%LOCALAPPDATA%\Cauce`. Pour désinstaller, fermez Cauce et supprimez son dossier. **Borrar datos locales** efface la bibliothèque et ses fichiers de récupération, sans supprimer la musique.

Depuis les sources, Windows et le SDK .NET 10 sont nécessaires :

```powershell
dotnet run --project src/Cauce.Desktop
```

L’interface actuelle est en espagnol ; ces documents traduisent les instructions, pas l’application.

[Accueil](../../../README.fr.md) · [Guide en anglais](../../installation.md)
