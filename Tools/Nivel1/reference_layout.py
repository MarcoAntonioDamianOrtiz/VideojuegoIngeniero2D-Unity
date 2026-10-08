"""Placement rules for the residential blocks in the reference composition."""

import json
from pathlib import Path


HERE = Path(__file__).resolve().parent
HOME_PLAN = json.loads((HERE / "reference_home_layout.json").read_text())
HOMES = {home["name"]: home for home in HOME_PLAN["homes"]}
FENCE_NUMBERS = {
    # A few short wooden garden rails remain. Parcel boundaries are masonry.
    85, 87, 88, 90, 91, 93,
}
DISABLED_GROUPS = {
    "Cercas_Parque_Norte", "Cercas_Parque_Sur", "Cercas_Patios",
    "Muros_Patios_Norte", "Muros_Patios_Oeste", "Muros_Huertos_Este",
    "Muros_Casas_Sur", "Arboles_Parque",
    "Muros_Parcelas_Norte", "Muros_Parcelas_Oeste", "Muros_Parcelas_Sur",
    "Muro_Parcela_Norte", "Muro_Parcela_Oeste", "Muro_Parcela_Sur",
    "Muro_Parcela_Vertical", "Muro_Parcela_Parque", "Muro_Parcela_Callejon",
}


def keep_placement(item):
    name = item["name"]
    sprite = item["sprite"]
    if name.startswith("Vivienda_"):
        return name in HOMES
    if name == "Mural_Aqui_Somos_Ciudad":
        return False
    if sprite.endswith("/cerca_madera.png"):
        try:
            number = int(name.rsplit("_", 1)[1])
        except (ValueError, IndexError):
            return False
        return number in FENCE_NUMBERS
    if sprite.endswith(("/cerca_rota.png", "/valla_obras.png")):
        return False
    return True


def reference_house(item):
    """Apply the surveyed location, size and facade to a kept home."""
    if item["name"] in HOMES:
        item.update(HOMES[item["name"]])


def keep_group(group):
    return group["name"] not in DISABLED_GROUPS
