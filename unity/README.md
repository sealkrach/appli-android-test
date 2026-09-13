# PolitiRush — projet Unity

Port du jeu vers Unity, conformément à `docs/ADR-001-moteur-graphique.md`.

## Ce qui est là

```
unity/
  Assets/PolitiRush/
    Core/       Moteur de règles en C# pur, traduit ligne à ligne du module Kotlin `core`.
                Aucune dépendance Unity (asmdef avec noEngineReferences). 30 tests NUnit.
    Tests/Editor/  Les tests NUnit (Unity Test Framework, mode Edit).
    Runtime/    Scripts de scène : GameController (pas fixe 60 Hz), SceneSync (état → objets),
                CharacterBuilder (personnages en primitives, placeholder des vrais modèles),
                vues du héros / des cibles / des portes, armes, HUD uGUI, entrée tactile,
                secousse de caméra, sauvegarde PlayerPrefs, enchaînement des parties.
    Editor/     SceneBuilder : menu « PolitiRush > Construire la scène de jeu ».
    Shaders/    Toon.shader : trois paliers + contour par coque inversée (pipeline intégré).
  Packages/manifest.json   Paquets requis (URP, Test Framework, TextMeshPro, uGUI).
  ProjectSettings/ProjectVersion.txt   Unity 2022.3 LTS.
  Tools/CoreCheck/   Vérification du moteur hors Unity : `dotnet run` compile Core + Tests.
```

## Première ouverture

1. Installer Unity Hub, puis Unity **2022.3 LTS** avec le module *Android Build Support*
   (SDK, NDK et OpenJDK inclus).
2. Unity Hub > *Add project from disk* > choisir le dossier `unity/`.
3. À l'ouverture, Unity importe les paquets du manifeste (une à deux minutes).
4. Menu **PolitiRush > Construire la scène de jeu**. La scène est créée dans
   `Assets/PolitiRush/Scenes/Game.unity` et s'ouvre.
5. **Play**. La partie démarre seule (héros Jean Rambeau, niveau Le Palais). Glisser à la
   souris pour se déplacer, espace ou bouton rond pour le spécial. Fin de partie :
   relance automatique après 2,5 s.
6. **Window > General > Test Runner > EditMode > Run All** : les 30 tests du moteur.

Pour changer de héros ou de niveau avant Play : objet `Jeu` > composant `GameController`
> `HeroId` (`rambeau`, `balbo`, `machete`, `chauve_souris`, `papa_particulier`,
`transporteur`) et `LevelId` (`palais`, `carburant`, `aeroport`).

## Build Android

File > Build Settings > Android > *Switch Platform*, puis *Build*. Pour un premier APK,
rien d'autre n'est nécessaire. Avant publication : identifiant de paquet dans Player
Settings, icône, orientation portrait verrouillée, signature.

## Ce qui reste à faire (voir docs/ROADMAP.md)

- Basculer sur URP et remplacer `Toon.shader` par un Shader Graph toon avec contour,
  puis activer bloom et vignette (Volume de post-traitement).
- Menu de sélection du niveau et du héros, boutique (le moteur `MetaProgress` est prêt).
- Remplacer `CharacterBuilder` par les modèles et animations du graphiste : un prefab par
  personnage avec des enfants nommés `ArmL`, `ArmR`, `LegL`, `LegR`, `Head` suffit pour
  que `CharacterAnimator` continue de fonctionner ; ensuite, Animator et ragdoll.
- Décor par niveau (pompes à essence, avions) : aujourd'hui seul le décor « Palais »
  est construit par `SceneBuilder`.
- Sons, vibrations, pubs, analytics.

## Vérifier le moteur sans Unity

```
cd unity/Tools/CoreCheck && dotnet run
```

Compile `Assets/PolitiRush/Core` et `Assets/PolitiRush/Tests/Editor` avec un mini-shim
NUnit et exécute les tests par réflexion. Utile en CI et sur un poste sans Unity.
