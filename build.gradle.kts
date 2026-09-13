// Fichier racine volontairement minimal.
// Les plugins Android sont déclarés dans app/build.gradle.kts (via le catalogue
// gradle/libs.versions.toml) pour que le module :core reste compilable et
// testable sans SDK Android ni accès au dépôt Google.
plugins {
    alias(libs.plugins.kotlin.jvm) apply false
}
