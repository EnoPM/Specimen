using System.Linq;
using System.Text.Json;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UniverseLib.Input;

namespace SpecimenDemo;

public class KeyBindsBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.GetKeyDown(KeyCode.F3))
        {
            if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data?.PlayerName == null) return;
            var player = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => !p.AmOwner);
            if (!player) return;
            player.RpcSetVisualName(ColorHelpers.Colorize(Color.yellow, PlayerControl.LocalPlayer.Data.PlayerName));
        }

        if (InputManager.GetKeyDown(KeyCode.F4))
        {
            var deserializerMethod = typeof(JsonSerializer).GetMethods().FirstOrDefault(x => x.Name == nameof(JsonSerializer.Deserialize) && x.ContainsGenericParameters && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(string));
            System.Console.WriteLine($"DeserializerMethod: {deserializerMethod != null}");
        }
    }
}