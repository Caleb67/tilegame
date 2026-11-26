
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomFontInputValidator", menuName = "Scriptable Objects/CustomFontInputValidator")]
public class CustomFontInputValidator : TMP_InputValidator
{
    [SerializeField] TMP_SpriteAsset spriteAsset;
    public override char Validate(ref string text, ref int pos, char ch) {
        var modifiedCh = ch switch {
            'x' => 'x',
            'u' => 'u',
            _ => char.ToUpperInvariant(ch)
        };
        var validChars = this.spriteAsset.spriteCharacterTable.Select(
            spriteChar => (char) spriteChar.unicode
        );
        if (validChars.Contains(modifiedCh)) {
            text += modifiedCh;
            pos++;
            return modifiedCh;
        }
        else return (char)/*invalid*/0;
    }
}