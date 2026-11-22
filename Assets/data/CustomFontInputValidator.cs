
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomFontInputValidator", menuName = "Scriptable Objects/CustomFontInputValidator")]
public class CustomFontInputValidator : TMP_InputValidator
{
    public TMP_SpriteAsset spriteAsset;
    public override char Validate(ref string text, ref int pos, char ch) {
        var upperCh = ch.ToString().ToUpper().ToCharArray().First();
        var validChars = this.spriteAsset.spriteCharacterTable.Select(
            spriteChar => (char) spriteChar.unicode
        );
        if (validChars.Contains(upperCh)) {
            text += upperCh;
            pos++;
            return upperCh;
        }
        else return (char)/*invalid*/0;
    }
}