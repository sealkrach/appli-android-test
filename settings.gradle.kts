pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "politirush"

// Le module `core` est du Kotlin pur : il se compile et se teste sans SDK Android.
include(":core")

// Le module `app` (Android + Compose) n'est inclus que si un SDK Android est
// disponible, pour que `gradle :core:test` reste utilisable sur n'importe quelle
// machine (CI légère, poste sans Android Studio).
val hasAndroidSdk = System.getenv("ANDROID_HOME") != null ||
    System.getenv("ANDROID_SDK_ROOT") != null ||
    file("local.properties").let { it.exists() && it.readText().contains("sdk.dir") }
if (hasAndroidSdk) {
    include(":app")
} else {
    logger.lifecycle("SDK Android introuvable : module :app ignoré (seul :core est configuré).")
}
