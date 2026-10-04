# Compilation et publications

Windows et le SDK .NET 10 sont nécessaires. Depuis la racine :

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet build src/Cauce.Desktop -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1 -Runtime win-x64
./build/cauce.ps1 -Runtime win-arm64
./build/release.ps1 -Version 0.6.0-rc.1
```

Les suites vérifient bibliothèque, file, identité et états WPF. Les tests synthétiques ne valident ni les fournisseurs réels ni les performances sur d’autres machines.

Les scripts produisent des ZIP portables avec runtime et sommes de contrôle. Ils restent non signés sauf utilisation d’un certificat autorisé et vérification de la signature. Aucun installateur ni paquet Store n’est fourni.

`build/cauce.ps1` accepte `-Version` et écrit dans `artifacts/cauce/`. `build/release.ps1` cible x64 et ARM64 par défaut et rassemble ZIP et `SHA256SUMS.txt` dans `artifacts/release/`.

`.github/workflows/cauce.yml` conserve des artefacts de vérification. `release.yml`, déclenché manuellement ou par une étiquette de version, prépare les paquets et crée une publication **en brouillon**. Vérifiez version, architecture, checksums, résultats et notes avant publication. La [politique de signature](code-signing-policy.md) régit toute demande de signature.

[Accueil](../../../README.fr.md) · [Guide en anglais](../../releasing.md)
