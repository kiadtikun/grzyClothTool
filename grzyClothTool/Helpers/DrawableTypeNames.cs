namespace grzyClothTool.Helpers;

public static class DrawableTypeNames
{
    public static string GetName(string code) => code switch
    {
        "head" => "Head",
        "berd" => "Masks",
        "hair" => "Hair Styles",
        "uppr" => "Torsos",
        "lowr" => "Legs",
        "hand" => "Bags and Parachutes",
        "feet" => "Shoes",
        "teef" => "Accessories",
        "accs" => "Undershirts",
        "task" => "Body Armors",
        "decl" => "Decals",
        "jbib" => "Tops",
        "p_head" => "Hats",
        "p_eyes" => "Glasses",
        "p_ears" => "Ears",
        "p_mouth" => "Mouth",
        "p_lhand" => "Left Hand",
        "p_rhand" => "Right Hand",
        "p_lwrist" => "Watches",
        "p_rwrist" => "Bracelets",
        "p_hip" => "Hip",
        "p_lfoot" => "Left Foot",
        "p_rfoot" => "Right Foot",
        "p_ph_l_hand" => "Left Hand Physics",
        "p_ph_r_hand" => "Right Hand Physics",
        _ => code ?? string.Empty
    };

    public static string GetLabel(string code)
    {
        var name = GetName(code);
        return name == code ? name : $"{name} [{code}]";
    }
}
