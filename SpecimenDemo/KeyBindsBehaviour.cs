using System.Linq;
using System.Text.Json;
using AmongUsSpecimen.Extensions;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Utils;
using UnityEngine;

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
            PlayerControl.LocalPlayer.RpcTeleportTo(player);
        }

        if (InputManager.GetKeyDown(KeyCode.F4))
        {
            var deserializerMethod = typeof(JsonSerializer).GetMethods().FirstOrDefault(x => x.Name == nameof(JsonSerializer.Deserialize) && x.ContainsGenericParameters && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(string));
            System.Console.WriteLine($"DeserializerMethod: {deserializerMethod != null}");
        }
    }
}