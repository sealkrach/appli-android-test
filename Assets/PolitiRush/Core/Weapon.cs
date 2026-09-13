namespace PolitiRush.Core
{
    /// <summary>Arme du héros, déduite du multiplicateur de tir. Miroir de Weapon.kt.</summary>
    public enum Weapon { Main, LancePierre, Canon, Tank }

    public static class WeaponInfo
    {
        public static string Label(this Weapon w) => w switch
        {
            Weapon.Main => "À la main", Weapon.LancePierre => "Lance-pierre", Weapon.Canon => "Canon à tomates", _ => "Tank à tomates",
        };

        public static int MinFirepower(this Weapon w) => w switch { Weapon.Main => 1, Weapon.LancePierre => 5, Weapon.Canon => 13, _ => 33 };

        public static Weapon ForFirepower(int firepower)
        {
            if (firepower >= Weapon.Tank.MinFirepower()) return Weapon.Tank;
            if (firepower >= Weapon.Canon.MinFirepower()) return Weapon.Canon;
            if (firepower >= Weapon.LancePierre.MinFirepower()) return Weapon.LancePierre;
            return Weapon.Main;
        }
    }
}
