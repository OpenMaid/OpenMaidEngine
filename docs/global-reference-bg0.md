<!-- DO NOT EDIT -- generated from vm-map/globals-bg0.toml by tools/globals_build.py --build -->
# Global Variable Reference — bg0 (generated)

3676 globals (422 curated, 3254 auto shape-inferred). Source of truth: `vm-map/globals-bg0.toml`.

## Tables

Open Maid Script tables (design spec §5.3): `table[record].field` is the field's array element.

### unit[1000]

Built by: `boot`.

EBINIT units keyed by unit id: every enemy, and the body a party character fights with (character_unit_ids), with CVINIT's voice setting by unit. Nothing of it is saved; UNITECH copies the base level, base stats, starting ranks and equipment into the character when it joins.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | unit_names | `GS 0x9fe` | `string[1000]` | |
| full_name | unit_full_names | `GS 0x616` | `string[1000]` | |
| epithet | unit_epithets | `GS 0xde6` | `string[1000]` | |
| description | unit_description_lines | `GS 0x11ce` | `string[1000][2]` | |
| voice_clips | unit_voice_clips | `0x9540d` | `int[1000][30]` | |
| pictures | unit_pictures | `0x9c93d` | `int[1000][3]` | pictures |
| battle_sprites | unit_battle_sprites | `0x9d4f5` | `int[1000][10]` | pictures |
| status_portrait | unit_status_portraits | `0x9fc05` | `int[1000]` | pictures |
| book_sort_key | unit_book_sort_keys | `0x9ffed` | `int[1000]` | |
| fs_temperament | unit_fs_temperaments | `0xa03d5` | `int[1000]` | |
| kind | unit_kinds | `0xa07bd` | `int[1000]` | |
| boss_class | unit_boss_classes | `0xa0ba5` | `int[1000]` | |
| size | unit_sizes | `0xa0f8d` | `int[1000]` | |
| defense_element | unit_defense_elements | `0xa175d` | `int[1000]` | `element` |
| weapon | unit_initial_weapons | `0xa1b45` | `int[1000]` | `item` |
| shield | unit_initial_shields | `0xa1f2d` | `int[1000]` | `item` |
| armor | unit_initial_armors | `0xa2315` | `int[1000]` | `item` |
| accessory | unit_initial_accessories | `0xa26fd` | `int[1000]` | `item` |
| equip_kinds | unit_equip_kind_flags | `0xa2ae5` | `int[1000]` | |
| skill_categories | unit_skill_categories | `0xa2ecd` | `int[1000][9]` | `item_class` |
| skill_category_open | unit_skill_category_open_flags | `0xa51f5` | `int[1000][9]` | |
| skill_start_ranks | unit_skill_start_ranks | `0xa751d` | `int[1000][9]` | |
| skill_max_ranks | unit_skill_max_ranks | `0xa9845` | `int[1000][9]` | |
| support_effect | unit_support_effects | `0xabb6d` | `int[1000]` | `skill_support` |
| base_level | unit_base_levels | `0xad2dd` | `int[1000]` | |
| max_level | unit_max_levels | `0xad6c5` | `int[1000]` | |
| level_catchup_divisor | unit_level_catchup_divisors | `0xadaad` | `int[1000]` | |
| starting_fs | unit_starting_fs | `0xade95` | `int[1000]` | |
| max_fs | unit_max_fs | `0xae27d` | `int[1000]` | |
| max_hp | unit_max_hp | `0xae665` | `int[1000]` | |
| max_mp | unit_max_mp | `0xaea4d` | `int[1000]` | |
| max_tp | unit_max_tp | `0xaee35` | `int[1000]` | |
| attacks | unit_attacks | `0xaf21d` | `int[1000]` | |
| defenses | unit_defenses | `0xaf605` | `int[1000]` | |
| magic_attacks | unit_magic_attacks | `0xaf9ed` | `int[1000]` | |
| magic_defenses | unit_magic_defenses | `0xafdd5` | `int[1000]` | |
| attack_counts | unit_attack_counts | `0xb01bd` | `int[1000]` | |
| defense_counts | unit_defense_counts | `0xb05a5` | `int[1000]` | |
| physical_resistances | unit_physical_resistances | `0xb098d` | `int[1000]` | |
| mental_resistances | unit_mental_resistances | `0xb0d75` | `int[1000]` | |
| physical_speeds | unit_physical_speeds | `0xb115d` | `int[1000]` | |
| mental_speeds | unit_mental_speeds | `0xb1545` | `int[1000]` | |
| luck | unit_luck | `0xb192d` | `int[1000]` | |
| max_hp_growth | unit_max_hp_growth | `0xb1d15` | `int[1000]` | |
| max_mp_growth | unit_max_mp_growth | `0xb20fd` | `int[1000]` | |
| max_tp_growth | unit_max_tp_growth | `0xb24e5` | `int[1000]` | |
| attack_growth | unit_attack_growth | `0xb28cd` | `int[1000]` | |
| defense_growth | unit_defense_growth | `0xb2cb5` | `int[1000]` | |
| magic_attack_growth | unit_magic_attack_growth | `0xb309d` | `int[1000]` | |
| magic_defense_growth | unit_magic_defense_growth | `0xb3485` | `int[1000]` | |
| attack_count_growth | unit_attack_count_growth | `0xb386d` | `int[1000]` | |
| defense_count_growth | unit_defense_count_growth | `0xb3c55` | `int[1000]` | |
| physical_resistance_growth | unit_physical_resistance_growth | `0xb403d` | `int[1000]` | |
| mental_resistance_growth | unit_mental_resistance_growth | `0xb4425` | `int[1000]` | |
| physical_speed_growth | unit_physical_speed_growth | `0xb480d` | `int[1000]` | |
| mental_speed_growth | unit_mental_speed_growth | `0xb4bf5` | `int[1000]` | |
| luck_growth | unit_luck_growth | `0xb4fdd` | `int[1000]` | |
| tactic_routines | unit_tactic_routines | `0xb53c5` | `int[1000][9]` | |
| action_ids | unit_action_ids | `0xb76ed` | `int[1000][6]` | `item` |
| exp_curve | unit_exp_curves | `0xb8e5d` | `int[1000]` | |
| exp_rate | unit_exp_rates | `0xb9245` | `int[1000]` | |
| exp_rewards | unit_exp_rewards | `0xb962d` | `int[1000][2]` | |
| money | unit_money | `0xb9dfd` | `int[1000]` | |
| drop_chances | unit_drop_chances | `0xba1e5` | `int[1000][5]` | |
| drop_items | unit_drop_items | `0xbb56d` | `int[1000][5]` | `item` |
| voice_setting_id | unit_voice_settings | `0x91441` | `int[1000]` | `voice_setting` |

### lead_form[10]

Built by: `boot`.

CBINIT rows keyed by セリカ's unit lead_unit_id (1 to 4, set from the world map by CHANGECHAPTER@0x00006): the lowest value and the raising cost of each of his seven parameters, and his HP, MP and TP percentage. Not saved; CHANGECHAPTER raises the saved lead_parameters to the minimums at a chapter change.

| field | global | address | type |
|---|---|---|---|
| strength_minimum | lead_strength_minimums | `0x95376` | `int[10]` |
| vitality_minimum | lead_vitality_minimums | `0x9538a` | `int[10]` |
| agility_minimum | lead_agility_minimums | `0x9539e` | `int[10]` |
| dexterity_minimum | lead_dexterity_minimums | `0x953b2` | `int[10]` |
| intelligence_minimum | lead_intelligence_minimums | `0x953c6` | `int[10]` |
| spirit_minimum | lead_spirit_minimums | `0x953da` | `int[10]` |
| charm_minimum | lead_charm_minimums | `0x953ee` | `int[10]` |
| strength_cost_rate | lead_strength_cost_rates | `0x95380` | `int[10]` |
| vitality_cost_rate | lead_vitality_cost_rates | `0x95394` | `int[10]` |
| agility_cost_rate | lead_agility_cost_rates | `0x953a8` | `int[10]` |
| dexterity_cost_rate | lead_dexterity_cost_rates | `0x953bc` | `int[10]` |
| intelligence_cost_rate | lead_intelligence_cost_rates | `0x953d0` | `int[10]` |
| spirit_cost_rate | lead_spirit_cost_rates | `0x953e4` | `int[10]` |
| charm_cost_rate | lead_charm_cost_rates | `0x953f8` | `int[10]` |
| pool_rate | lead_pool_rates | `0x95402` | `int[10]` |

### character[100]

Built by: `boot`.

Characters keyed by character id, all saved: CHINIT's unit (written at every boot by INIT, so a change reaches only new games) and the state the game keeps for the character.

| field | global | address | type | refers to |
|---|---|---|---|---|
| unit_id | character_unit_ids | `0x8b236` | `int[100]` | `unit` |
| flags | character_flags | `0x8b1c1` | `int[100]` | |
| level | character_levels | `0x8b2fe` | `int[100]` | |
| experience | character_experience | `0x8b362` | `int[100]` | |
| hp | character_hp | `0x8b42a` | `int[100]` | |
| max_hp | character_max_hp | `0x8b48e` | `int[100]` | |
| mp | character_mp | `0x8b4f2` | `int[100]` | |
| max_mp | character_max_mp | `0x8b556` | `int[100]` | |
| tp | character_tp | `0x8b5ba` | `int[100]` | |
| max_tp | character_max_tp | `0x8b61e` | `int[100]` | |
| attacks | character_attacks | `0x8b682` | `int[100]` | |
| defenses | character_defenses | `0x8b6e6` | `int[100]` | |
| magic_attacks | character_magic_attacks | `0x8b74a` | `int[100]` | |
| magic_defenses | character_magic_defenses | `0x8b7ae` | `int[100]` | |
| attack_counts | character_attack_counts | `0x8b812` | `int[100]` | |
| defense_counts | character_defense_counts | `0x8b876` | `int[100]` | |
| physical_resistances | character_physical_resistances | `0x8b8da` | `int[100]` | |
| mental_resistances | character_mental_resistances | `0x8b93e` | `int[100]` | |
| physical_speeds | character_physical_speeds | `0x8b9a2` | `int[100]` | |
| mental_speeds | character_mental_speeds | `0x8ba06` | `int[100]` | |
| luck | character_luck | `0x8ba6a` | `int[100]` | |
| skill_ranks | character_skill_ranks | `0x8bace` | `int[100][9]` | |
| skill_experience | character_skill_experience | `0x8be52` | `int[100][9]` | |
| weapon | character_weapons | `0x8c1d6` | `int[100]` | |
| shield | character_shields | `0x8c302` | `int[100]` | |
| armor | character_armors | `0x8c366` | `int[100]` | |
| accessory | character_accessories | `0x8c3ca` | `int[100]` | |
| tactic | character_tactics | `0x8e36e` | `int[100]` | |

### speaker_entry[1000]

Built by: `boot`.

CNINIT speakers keyed by the speaker id, the same number as the speaker's unit.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | speaker_names | `GS 0xe0` | `string[1000]` | |
| voice_unit | speaker_voice_units | `0x91059` | `int[1000]` | `unit` |

### voice_setting[30]

Built by: `boot`.

CVINIT voice settings keyed by setting id (0 to 15), the rows of the 設定 voice page.

| field | global | address | type | refers to |
|---|---|---|---|---|
| sample | voice_setting_samples | `0x91847` | `int[30]` | |
| unit_id | voice_setting_units | `0x91829` | `int[30]` | `unit` |

### profile[100]

Built by: `boot`.

CIINIT profiles keyed by profile id: the entries of the 人物 book (INFO2).

| field | global | address | type | refers to |
|---|---|---|---|---|
| unit_id | profile_unit_ids | `0x3acd41` | `int[100]` | `unit` |
| name | profile_names | `GS 0x8177` | `string[100]` | |
| picture | profile_pictures | `0x3acda5` | `int[100]` | pictures |
| picture_x | profile_picture_x | `0x3ace09` | `int[100]` | |
| picture_y | profile_picture_y | `0x3ace6d` | `int[100]` | |

### item[10000]

Built by: `boot`.

ITINIT records keyed by item id (742 written): key items 1..35, consumables 101..303, weapons 1001..1711, armour 2001..2942, magic 3001..3137, arts 4001..4057, enemy actions 5001..7207, special 9992..9999 (9999 素手). `item[101].name` is item_names[101].

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | item_names | `GS 0x2e70` | `string[10000]` | |
| note | item_notes | `GS 0x5580` | `string[10000]` | |
| sort_key | item_sort_keys | `0x12558d` | `int[10000]` | |
| rank | item_ranks | `0x12a3ad` | `int[10000]` | |
| category | item_categories | `0x12cabd` | `int[10000]` | |
| class | item_classes | `0x12f1cd` | `int[10000]` | `item_class` |
| damage_kind | item_damage_kinds | `0x1318dd` | `int[10000]` | |
| attack_element | item_attack_elements | `0x133fed` | `int[10000]` | `element` (a value above 8 names none) |
| defense_element | item_defense_elements | `0x1366fd` | `int[10000]` | `element` |
| exclusive_users | item_exclusive_users | `0x138e0d` | `int[10000][100]` | `character` |
| required_item | item_required_items | `0x22d04d` | `int[10000][5]` | `item` |
| cursed | item_cursed_flags | `0x23939d` | `int[10000]` | |
| target_kind | item_target_kinds | `0x23baad` | `int[10000]` | |
| area_width | item_area_widths | `0x23e1bd` | `int[10000]` | |
| area_height | item_area_heights | `0x2408cd` | `int[10000]` | |
| charge_time | item_charge_times | `0x242fdd` | `int[10000]` | |
| min_charge_time | item_min_charge_times | `0x2456ed` | `int[10000]` | |
| recovery_time | item_recovery_times | `0x247dfd` | `int[10000]` | |
| min_recovery_time | item_min_recovery_times | `0x24a50d` | `int[10000]` | |
| condition_levels | item_condition_levels | `0x24cc1d` | `int[10000][40]` | |
| equip_condition_levels | item_equip_condition_levels | `0x2ae69d` | `int[10000][40]` | |
| accuracy | item_accuracy_bonuses | `0x31011d` | `int[10000]` | |
| evasion | item_evasion_bonuses | `0x31282d` | `int[10000]` | |
| physical_attack | item_physical_attack_bonuses | `0x314f3d` | `int[10000]` | |
| physical_defense | item_physical_defense_bonuses | `0x31764d` | `int[10000]` | |
| attack_count | item_attack_count_bonuses | `0x319d5d` | `int[10000]` | |
| defense_count | item_defense_count_bonuses | `0x31c46d` | `int[10000]` | |
| magic_attack | item_magic_attack_bonuses | `0x31eb7d` | `int[10000]` | |
| magic_defense | item_magic_defense_bonuses | `0x32128d` | `int[10000]` | |
| magic_power | item_magic_power_bonuses | `0x32399d` | `int[10000]` | |
| physical_resistance | item_physical_resistance_bonuses | `0x3260ad` | `int[10000]` | |
| mental_resistance | item_mental_resistance_bonuses | `0x3287bd` | `int[10000]` | |
| physical_speed | item_physical_speed_bonuses | `0x32aecd` | `int[10000]` | |
| mental_speed | item_mental_speed_bonuses | `0x32d5dd` | `int[10000]` | |
| luck | item_luck_bonuses | `0x32fced` | `int[10000]` | |
| critical | item_critical_bonuses | `0x3323fd` | `int[10000]` | |
| max_hp | item_max_hp_bonuses | `0x34356d` | `int[10000]` | |
| max_mp | item_max_mp_bonuses | `0x345c7d` | `int[10000]` | |
| max_tp | item_max_tp_bonuses | `0x34838d` | `int[10000]` | |
| hp | item_hp_amounts | `0x334b0d` | `int[10000]` | |
| mp | item_mp_amounts | `0x33721d` | `int[10000]` | |
| tp | item_tp_amounts | `0x33992d` | `int[10000]` | |
| fs | item_fs_amounts | `0x33c03d` | `int[10000]` | |
| control_rate_change | item_control_rate_changes | `0x33e74d` | `int[10000]` | |
| random_amount | item_random_amounts | `0x34d1ad` | `int[10000]` | |
| animation | item_animations | `0x34f8bd` | `int[10000]` | `battle_animation` |
| buy_price | item_buy_prices | `0x351fcd` | `int[10000]` | |
| sell_price | item_sell_prices | `0x3546dd` | `int[10000]` | |
| menu_use | item_menu_use_masks | `0x356ded` | `int[10000]` | |
| count | item_counts | `0x8632a` | `int[10000]` | |
| obtained | item_obtained_flags | `0x88a3a` | `int[10000]` | |

### condition[40]

Built by: `boot`.

ILINIT conditions keyed by condition id (1 to 36), five levels each. `condition[7].names` are 呪い1 to 呪い5.

| field | global | address | type |
|---|---|---|---|
| names | condition_names | `GS 0x7c90` | `string[40][5]` |
| descriptions | condition_descriptions | `GS 0x7d58` | `string[40][10]` |
| resistance | condition_resistances | `0x3594ff` | `int[40][5]` |
| affinity_columns | condition_affinity_columns | `0x3595c7` | `int[40][5]` |
| cure_bits | condition_cure_bits | `0x35968f` | `int[40][5]` |
| durations | condition_durations | `0x359757` | `int[40][5]` |
| hit_modifiers | condition_hit_modifiers | `0x35981f` | `int[40][5]` |
| evasion_modifiers | condition_evasion_modifiers | `0x3598e7` | `int[40][5]` |
| physical_attack_modifiers | condition_physical_attack_modifiers | `0x3599af` | `int[40][5]` |
| physical_defense_modifiers | condition_physical_defense_modifiers | `0x359a77` | `int[40][5]` |
| attack_count_modifiers | condition_attack_count_modifiers | `0x359b3f` | `int[40][5]` |
| defense_count_modifiers | condition_defense_count_modifiers | `0x359c07` | `int[40][5]` |
| magic_attack_modifiers | condition_magic_attack_modifiers | `0x359ccf` | `int[40][5]` |
| magic_defense_modifiers | condition_magic_defense_modifiers | `0x359d97` | `int[40][5]` |
| magic_power_modifiers | condition_magic_power_modifiers | `0x359e5f` | `int[40][5]` |
| physical_resistance_modifiers | condition_physical_resistance_modifiers | `0x359f27` | `int[40][5]` |
| mental_resistance_modifiers | condition_mental_resistance_modifiers | `0x359fef` | `int[40][5]` |
| physical_speed_modifiers | condition_physical_speed_modifiers | `0x35a0b7` | `int[40][5]` |
| mental_speed_modifiers | condition_mental_speed_modifiers | `0x35a17f` | `int[40][5]` |
| luck_modifiers | condition_luck_modifiers | `0x35a247` | `int[40][5]` |
| critical_modifiers | condition_critical_modifiers | `0x35a30f` | `int[40][5]` |
| hp_changes | condition_hp_changes | `0x35a3d7` | `int[40][5]` |
| mp_changes | condition_mp_changes | `0x35a49f` | `int[40][5]` |
| tp_changes | condition_tp_changes | `0x35a567` | `int[40][5]` |
| fs_changes | condition_fs_changes | `0x35a62f` | `int[40][5]` |
| listed | condition_listed_flags | `0x35a6f7` | `int[40]` |

### skill_support[100]

Built by: `boot`.

SSINIT support effects keyed by id (1 to 12): names and two description lines only.

| field | global | address | type |
|---|---|---|---|
| name | skill_support_names | `GS 0x7ee8` | `string[100]` |
| description | skill_support_descriptions | `GS 0x7f4c` | `string[100][2]` |

### element[20]

Built by: `boot`.

AFINIT elements keyed by defense element id, 1 to 14 named; row 0, 100 throughout, has no name. Each row is how the element takes each attack element (element_affinities, its columns named by element_attack_names). Attack element n is element n up to 8.

| field | global | address | type |
|---|---|---|---|
| name | element_defense_names | `GS 0x8028` | `string[20]` |
| affinities | element_affinities | `0x3866ad` | `int[20][20]` |

### item_class[60]

Built by: `boot`.

AFINIT classes keyed by class id (ITINIT's item_classes): item kinds, equipment kinds, magic schools and art schools.

| field | global | address | type |
|---|---|---|---|
| name | item_class_names | `GS 0x803c` | `string[60]` |
| short_name | item_class_short_names | `GS 0x8078` | `string[60]` |
| equipment_note | item_class_equipment_notes | `GS 0x80b4` | `string[60]` |
| icon | item_class_icons | `0x38683d` | `int[60]` |
| required_weapons | item_class_required_weapon_masks | `0x386879` | `int[60]` |
| rank_exps | item_class_rank_exps | `0x3868b5` | `int[60][6]` |

### formation[9]

Built by: `boot`.

AFINIT formations keyed by formation id 1..8 (the party's is formation_current; formation_learned_mask has a bit for each learned); names and effect text only.

| field | global | address | type |
|---|---|---|---|
| name | formation_names | `GS 0x80f0` | `string[9]` |
| effect_lines | formation_effect_lines | `GS 0x80f9` | `string[9][6]` |

### treasure[10000]

Built by: `boot`.

TRINIT treasures keyed by treasure id (532 written: 1..1207, 2000..2082, and the starting kits 9998 and 9999): five slots of an item and a count, or money. A map object names its treasure; GETTREASURE opens it.

| field | global | address | type | refers to |
|---|---|---|---|---|
| items | treasure_items | `0x10cc93` | `int[10000][5]` | `item` |
| counts | treasure_counts | `0x118fe3` | `int[10000][5]` | |
| opened | treasure_opened | `0x83c1a` | `int[10000]` | |

### shop_slot[20]

Built by: `shop`.

SHINIT goods of the shop in play, keyed by 10 * page + slot; SHINIT refills them for shop_id each time SHOP opens.

| field | global | address | type | refers to |
|---|---|---|---|---|
| item | shop_items | `0xc1e5d` | `int[20]` | `item` |

### routine[1000]

Built by: `boot`.

RTINIT battle routines keyed by routine id (EBINIT's G[0xb53c5] names a unit's, one a tactic); each field holds 20 steps.

| field | global | address | type |
|---|---|---|---|
| step_kind | routine_step_kinds | `0x35a787` | `int[1000][20]` |
| step_chance | routine_step_chances | `0x35f5a7` | `int[1000][20]` |
| step_action | routine_step_actions | `0x3643c7` | `int[1000][20]` |
| step_arg | routine_step_args | `0x3691e7` | `int[1000][20]` |
| step_percent | routine_step_percents | `0x36e007` | `int[1000][20]` |
| step_target_count | routine_step_target_counts | `0x372e27` | `int[1000][20]` |
| step_flag | routine_step_flags | `0x37ca67` | `int[1000][20]` |
| step_limit | routine_step_limits | `0x381887` | `int[1000][20]` |

### battle_animation[10000]

Built by: `boot`.

BTANINIT2 battle animations keyed by animation id (ITINIT's G[0x34f8bd] names an action's).

| field | global | address | type |
|---|---|---|---|
| effects | battle_animation_effects | `0x38a9fa` | `int[10000][6]` |
| effect_starts | battle_animation_effect_starts | `0x39945a` | `int[10000][6]` |
| length | battle_animation_lengths | `0x3a7eba` | `int[10000]` |
| split_style | battle_animation_split_styles | `0x3aa5ca` | `int[10000]` |

### battle_effect[6]

Built by: `battle`.

BTANINIT's effects of the animation in play, keyed by the animation's effect slot, 0 to 5.

| field | global | address | type | refers to |
|---|---|---|---|---|
| picture | battle_effect_pictures | `0x3accda` | `int[6]` | pictures |
| still_kind | battle_effect_still_kinds | `0x3acce0` | `int[6]` | |
| blend_mode | battle_effect_blend_modes | `0x3acce6` | `int[6]` | |
| width | battle_effect_widths | `0x3accec` | `int[6]` | |
| height | battle_effect_heights | `0x3accf2` | `int[6]` | |
| sheet_columns | battle_effect_sheet_columns | `0x3accf8` | `int[6]` | |
| sheet_rows | battle_effect_sheet_rows | `0x3accfe` | `int[6]` | |
| frame_count | battle_effect_frame_counts | `0x3acd04` | `int[6]` | |
| length | battle_effect_lengths | `0x3acd0a` | `int[6]` | |
| placed | battle_effect_placed_flags | `0x3acd10` | `int[6]` | |
| x_offset | battle_effect_x_offsets | `0x3acd16` | `int[6]` | |
| y_offset | battle_effect_y_offsets | `0x3acd1c` | `int[6]` | |
| sound | battle_effect_sounds | `0x3acd22` | `int[6]` | |
| sound_delay | battle_effect_sound_delays | `0x3acd28` | `int[6]` | |
| cue_times | battle_effect_cue_times | `0x3acd2e` | `int[6][3]` | |

### encounter_entry[100]

Built by: `encounter`.

ENINIT's entries of the encounter set in play, keyed by entry number; entry 0 is the fallback.

| field | global | address | type |
|---|---|---|---|
| weight | encounter_entry_weights | `0x125334` | `int[100]` |
| gauge_low | encounter_entry_gauge_lows | `0x125398` | `int[100]` |
| gauge_high | encounter_entry_gauge_highs | `0x1253fc` | `int[100]` |
| formations | encounter_entry_formations | `0x125460` | `int[100][2]` |

### formation_slot[8]

Built by: `battle`.

ESINIT's slots of the formation in play, keyed by slot number.

| field | global | address | type |
|---|---|---|---|
| unit | formation_slot_units | `0x125529` | `int[8]` |
| first_column | formation_slot_first_columns | `0x125531` | `int[8]` |
| last_column | formation_slot_last_columns | `0x125539` | `int[8]` |
| first_row | formation_slot_first_rows | `0x125541` | `int[8]` |
| last_row | formation_slot_last_rows | `0x125549` | `int[8]` |
| chance | formation_slot_chances | `0x125551` | `int[8]` |

### arena_round[40]

Built by: `boot`.

ARINIT arena rounds keyed by 10 * rank + round (ranks 0 to 3, rounds 0 to 6; arena_rank_in_play, arena_round_in_play).

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | arena_round_names | `GS 0x1a20` | `string[40]` | |
| texts | arena_round_texts | `GS 0x1a48` | `string[40][4]` | |
| mark | arena_round_marks | `0xc1e7d` | `int[40]` | |
| enemy_level | arena_round_enemy_levels | `0xc1ea5` | `int[40]` | |
| formations | arena_round_formations | `0xc1ecd` | `int[40][2]` | |
| fee | arena_round_fees | `0xc1f1d` | `int[40]` | |
| prize | arena_round_prizes | `0xc1f45` | `int[40]` | |
| reward_items | arena_round_reward_items | `0xc1f6d` | `int[40][2]` | `item` |
| reward_counts | arena_round_reward_counts | `0xc1fbd` | `int[40][2]` | |
| required_level | arena_round_required_levels | `0xc200d` | `int[40]` | |
| party_limit | arena_round_party_limits | `0xc2035` | `int[40]` | |
| characters | arena_round_characters | `0xc205d` | `int[40][5]` | `character` |

### arena_rank[4]

Built by: `boot`.

ARINIT's cells of each arena rank, keyed by rank (arena_rank_in_play).

| field | global | address | type | refers to |
|---|---|---|---|---|
| picture | arena_rank_pictures | `0xc1e75` | `int[4]` | pictures |
| backdrop | arena_rank_backdrops | `0xc1e79` | `int[4]` | pictures |

### area[100]

Built by: `world_map`.

AMINIT's areas of the world map in play (story_part), keyed by place id (current_place_id): a map 1 to 49 (kind 1), a town from 50 (kind 2) or the junction 90. AMINIT clears and refills them each time ALLMAP or SHOWALLMAP opens, except area_town_music and area_names, which it only overwrites.

| field | global | address | type | refers to |
|---|---|---|---|---|
| state | area_states | `0x30b3` | `int[100]` | |
| name | area_names | `GS 0x199e` | `string[100]` | |
| pictures | area_pictures | `0xc1716` | `int[100][3]` | pictures |
| town_music | area_town_music | `0xc1842` | `int[100]` | |
| kind | area_kinds | `0xc1a36` | `int[100]` | |
| x | area_x | `0xc1a9a` | `int[100]` | |
| y | area_y | `0xc1afe` | `int[100]` | |
| up | area_up_neighbors | `0xc1b62` | `int[100]` | `area` |
| down | area_down_neighbors | `0xc1bc6` | `int[100]` | `area` |
| left | area_left_neighbors | `0xc1c2a` | `int[100]` | `area` |
| right | area_right_neighbors | `0xc1c8e` | `int[100]` | `area` |

### town_site[30]

Built by: `town`.

SIINIT's sites of the town in play (current_place_id), keyed by site number from 1; site 1 is the exit.

| field | global | address | type | refers to |
|---|---|---|---|---|
| state | town_site_states | `0x314a` | `int[30]` | |
| name | town_site_names | `GS 0x1a02` | `string[30]` | |
| pictures | town_site_pictures | `0xc1cf2` | `int[30][2]` | pictures |
| x | town_site_x | `0xc1d2e` | `int[30]` | |
| y | town_site_y | `0xc1d4c` | `int[30]` | |
| hot_left | town_site_hot_lefts | `0xc1d6a` | `int[30]` | |
| hot_top | town_site_hot_tops | `0xc1d88` | `int[30]` | |
| hot_width | town_site_hot_widths | `0xc1da6` | `int[30]` | |
| hot_height | town_site_hot_heights | `0xc1dc4` | `int[30]` | |
| neighbors | town_site_neighbors | `0xc1de2` | `int[30][4]` | `town_site` |

### map[50]

Built by: `boot`.

MPINIT field maps keyed by place id: the object sheet, the start, the encounter decay, the chapter-change keep and the floors (columns 1 to 9). The sections' scripts and pictures are RMINIT's.

| field | global | address | type | refers to |
|---|---|---|---|---|
| object_sheet | map_object_sheets | `0xc214d` | `int[50]` | pictures |
| start_floor | map_start_floors | `0xc217f` | `int[50]` | |
| start_section | map_start_sections | `0xc21b1` | `int[50]` | |
| start_x | map_start_x | `0xc21e3` | `int[50]` | |
| start_y | map_start_y | `0xc2215` | `int[50]` | |
| start_facing | map_start_facings | `0xc2247` | `int[50]` | |
| encounter_decay_distance | map_encounter_decay_distances | `0xc2279` | `int[50]` | |
| keeps_section_states | map_keeps_section_states | `0xc22ab` | `int[50]` | |
| floor_numbers | map_floor_numbers | `0xc22dd` | `int[50][100]` | |
| floor_names | map_floor_names | `GS 0x1ae8` | `string[50][100]` | |

### map_block[450]

Built by: `map`.

RMINIT's blocks of the map in play, keyed by 30 * floor + section (map_block_row, map_block_column).

| field | global | address | type | refers to |
|---|---|---|---|---|
| script | map_block_scripts | `0xc3665` | `int[450]` | |
| event_script | map_block_event_scripts | `0xc3827` | `int[450]` | |
| picture | map_block_pictures | `0xc39e9` | `int[450]` | pictures |
| picture_2 | map_block_pictures_2 | `0xc3bab` | `int[450]` | pictures |
| overlay_picture | map_block_overlay_pictures | `0xc3d6d` | `int[450]` | pictures |
| minimap_picture | map_block_minimap_pictures | `0xc3f2f` | `int[450]` | pictures |
| battle_backdrop | map_block_battle_backdrops | `0xc40f1` | `int[450]` | pictures |
| music | map_block_music | `0xc4475` | `int[450][3]` | |
| encounter_sets | map_block_encounter_sets | `0xc4b7d` | `int[450][2]` | |
| enemy_level | map_block_enemy_levels | `0xc4f01` | `int[450]` | |
| enemy_level_spread | map_block_enemy_level_spreads | `0xc50c3` | `int[450]` | |
| encounter_level | map_block_encounter_levels | `0xc5285` | `int[450]` | |

### land[100]

Built by: `boot`.

LAINIT land types keyed by the land under the player (field_land_id); 0 is no entry.

| field | global | address | type |
|---|---|---|---|
| footstep_sound | land_footstep_sounds | `0xd9c8e` | `int[100]` |
| sprite_column | land_sprite_columns | `0xd9cf2` | `int[100]` |
| speed_change | land_speed_changes | `0xd9d56` | `int[100]` |
| drift_x | land_drift_x | `0xd9dba` | `int[100]` |
| drift_y | land_drift_y | `0xd9e1e` | `int[100]` |
| drift_walkable | land_drift_walkable | `0xd9e82` | `int[100]` |
| encounter_level | land_encounter_levels | `0xd9fae` | `int[100]` |

### object_kind[1000]

Built by: `boot`.

OBINIT and OBINIT2 map object kinds, one id in both (map_object_kinds): the sprite rectangle, the depth, the wall polygon and what the object turns into.

| field | global | address | type |
|---|---|---|---|
| sprite_x | object_kind_sprite_x | `0xda012` | `int[1000]` |
| sprite_y | object_kind_sprite_y | `0xda3fa` | `int[1000]` |
| width | object_kind_widths | `0xda7e2` | `int[1000]` |
| height | object_kind_heights | `0xdabca` | `int[1000]` |
| depth_offset | object_kind_depth_offsets | `0xdafb2` | `int[1000]` |
| wall_point_count | object_kind_wall_point_counts | `0xdb39a` | `int[1000]` |
| wall_point_x | object_kind_wall_point_x | `0xdb782` | `int[1000][100]` |
| wall_point_y | object_kind_wall_point_y | `0xf3e22` | `int[1000][100]` |
| next_kind | object_kind_next_kinds | `0x10c4c2` | `int[1000]` |
| change_sound | object_kind_change_sounds | `0x10c8aa` | `int[1000]` |

### field_sprite[10]

Built by: `boot`.

CPINIT's field sheets of the player's character, keyed by セリカ's unit (lead_unit_id, 1 to 4), each a row of 30 by land sprite column.

| field | global | address | type | refers to |
|---|---|---|---|---|
| walk_sheets | field_sprite_walk_sheets | `0xc5447` | `int[10][30]` | pictures |
| jump_sheets | field_sprite_jump_sheets | `0xc5573` | `int[10][30]` | pictures |

### scene[10000]

Built by: `boot`.

SCINIT scenes keyed by scene id: SCJUMP or a map event names one, SYSTEM4 plays its script. 特別編 lays $1$SCINIT's 30 scenes over the base ones.

| field | global | address | type |
|---|---|---|---|
| script | scene_script_ids | `0xbc8f6` | `int[10000]` |
| offer_context | scene_offer_contexts | `0xbf006` | `int[10000]` |

### glossary_entry[200]

Built by: `boot`.

VIINIT glossary entries, ids 1 to 90, the screen INFO1; the text comes from VIMES.

| field | global | address | type | refers to |
|---|---|---|---|---|
| title | glossary_titles | `GS 0x81db` | `string[200]` | |
| unlock_scenes | glossary_unlock_scenes | `0x3aced2` | `int[200][3]` | `scene` (value − 1) |
| picture | glossary_pictures | `0x3ad12a` | `int[200]` | pictures |
| picture_x_offset | glossary_picture_x_offsets | `0x3ad1f2` | `int[200]` | |
| picture_y_offset | glossary_picture_y_offsets | `0x3ad2ba` | `int[200]` | |

### cg[2000]

Built by: `boot`.

CGINIT gallery CGs keyed by CG id, 1 to 715; CGMODE places each by tab, slot and variant.

| field | global | address | type | refers to |
|---|---|---|---|---|
| picture | cg_pictures | `0x9186b` | `int[2000][2]` | pictures |
| tab | cg_tabs | `0x9280b` | `int[2000]` | |
| slot | cg_slots | `0x92fdb` | `int[2000]` | |
| variant | cg_variants | `0x937ab` | `int[2000]` | |
| scroll_x | cg_scroll_x | `0x93f7b` | `int[2000]` | |
| scroll_y | cg_scroll_y | `0x9474b` | `int[2000]` | |

### replay[75]

Built by: `boot`.

SPINIT scene replays keyed by page * 15 + cell (5 pages of 15; HMODE shows 3).

| field | global | address | type |
|---|---|---|---|
| script | replay_scripts | `0x94f1b` | `int[75]` |

### Scope world

Ids 1 to 4. A data file reaches each table below as `world.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A world map, by story_part. AMINIT clears these tables and fills them from the branch for the world in play each time ALLMAP or SHOWALLMAP opens; at boot (world 0) it only clears.

| name | table |
|---|---|
| area | area |

### Scope town

Ids 50 to 99. A data file reaches each table below as `town.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A town, by current_place_id (an area id from 50; the area arrays hold 100, and the game's towns are 50 to 82, with 90 a junction whose SIINIT branch is empty). SIINIT clears these tables and fills them from the branch for the town in play on each TOWN visit.

| name | table |
|---|---|
| site | town_site |

### Scope dungeon

Ids 1 to 49. A data file reaches each table below as `dungeon.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A field map, by current_place_id (an MPINIT id; the game's are 1 to 41 and 49 in the special mode, and the per-map arrays hold 50). RMINIT clears these tables and fills them from the branch for the map in play each time FIELD starts and when a save made on a map is loaded.

| name | table |
|---|---|
| block | map_block |

## choice-output

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x90c1a` | special_edition_mode | high | investigation | Saved: the game mode, 0 通常 or 1 特別編. GAMESTART@0x7b stores the BUNKI choice G[0x94f67] (GAMESTART@0x1f..0x78). While it is set, $1$AUTORUN runs $1$AMINIT and $1$SIINIT (@0xf) and $1$SCINIT (@0x3a); ALLMAP@0x5a, TOWN@0x5d, FIELD@0x7b and CALLBACK_LOAD@0xd1 run $1$AMINIT, $1$SIINIT and $1$RMINIT in place of the base scripts; and every SCJUMP caller runs $1$SCJUMP (ALLMAP@0x31, FIELD@0x13b, TOWN@0x39, TOWN@0x522, BTL@0x706a, MENU@0x58a). It is 0 at the boot, so a new 特別編 game runs $1$CHINIT and $1$SCINIT itself (GAMESTART@0x3ab..0x3b5); INIT2 runs on every load after the saved prefix is back, so a 特別編 save loads those tables again. The same base also indexes shared-profile flags: [cg] an append CG's unlock ($1$SC0060@0x4b01, CGMODE@0x9b7), [800 + slot] a save slot's (SAVE@0xd13; not decoded). Values: 0 通常, 1 特別編. |
| `0x91869` | current_scene_id | high | investigation | The scene to play, an id of SCINIT. A SCJUMP rule (SCJUMP@0x6f), a map event (MPEV01B0102@0x1d5) or TITLE@0xa10 sets it; SYSTEM4@0x46d plays it and a scene script dispatches on it (SC0000@0x729). A scene's end sets 0 (SC0000@0x481a). Above the saved prefix. |

## counter

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x3170` | map_player_x | high | investigation | Saved: the player's x on the map; DRAWWALK@0x0 walks it toward map_player_target_x. |
| `0x3171` | map_player_target_x | high | investigation | Saved: the x the player walks to (DRAWWALK@0xa); MPINIT's start x is written here on entry (FIELD@0xba..0xf1). |
| `0x3172` | map_player_y | high | investigation | Saved: the player's y on the map; DRAWWALK@0x5 walks it toward map_player_target_y. |
| `0x3173` | map_player_target_y | high | investigation | Saved: the y the player walks to (DRAWWALK@0x11); MPINIT's start y is written here on entry. |
| `0x8632a` | item_counts | high | investigation | Saved: how many of each item the party holds, 0 to 999 (ADDITEM@0x8..0x5d); SHOP@0x10a3 and CHECKACT read it. |
| `0x8b159` | money | high | investigation | Saved: the party's money, kept within 0 and 9,999,999 (GETITEM@0x12f..0x15a, GAMESTART@0x62a); SHOP@0x1813 takes a purchase and SHOP@0x186d adds a sale. Outside this step's prefixes; another part may own it. |
| `0x8f568` | arena_rounds_won | high | investigation | Saved: how many rounds of the rank are won. ARENA starts the round there (ARENA@0xbb), refuses a round above it (ARENA@0x5e3) and adds one on a first win (ARENA@0x7b4..0x7d7); RESULT pays the prize only for that round (RESULT@0x2ef). |
| `0x38a787` | routine_step_run_counts | high | investigation | Battle work: how many times each step has acted for the battler (row) in this battle; RTN@0x1d5..0x1eb adds one, SETENP@0x38 clears them. |

## data-table

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x30b3` | area_states | high | investigation | Saved; AMINIT clears it and sets bit 0 (bit-set) under story-flag tests: bit 0 shows the area (ALLMAP@0x90, SHOWALLMAP@0x3e). Bit 1 would refuse entry with SE016.WAV (ALLMAP@0x734..0x751), but no script sets it. |
| `0x314a` | town_site_states | high | investigation | Saved; SIINIT clears it and sets bit 0 (bit-set) under story-flag tests: an open site, which TOWN draws and lets the cursor reach (TOWN@0x8f, 0x5b5). |
| `0x3182` | map_section_codes | high | investigation | Saved: the section each slot holds, map * 10000 + floor * 100 + section, found or taken at FIELD@0x1a7..0x230. CHANGECHAPTER@0x4a4..0x576 clears every slot unless map_keeps_section_states of the slot in play's map is 1. |
| `0x2cb86` | map_object_flags | med | investigation | Saved: each section slot's object flags; DRAWMAP@0x3c6 draws object k when bit 16 is set and bit 0 is clear. Other bits are not decoded. |
| `0x38ed6` | map_object_kinds | high | investigation | Saved: each section slot's object kinds, OBINIT ids (DRAWMAP@0x3e6); EVOLVEOBJ@0x1d..0xe3 replaces a kind with object_kind_next_kinds, 0 removing the object. |
| `0x45226` | map_object_x | high | investigation | Saved: each section slot's object x on the map; DRAWMAP@0x428 draws the sprite there, minus the scroll G[0xc57b3]. FIELD@0xefa adds it to the kind's wall points. |
| `0x51576` | map_object_y | high | investigation | Saved: each section slot's object y on the map (DRAWMAP@0x43a, minus the scroll G[0xc57b4]); y + object_kind_depth_offsets is the sort key (DRAWMAP@0x7f..0xc0). |
| `0x8b1c1` | character_flags | high | investigation | Saved. Bit 0: has joined (UNITECH@0x00057; INIT2@0x000b9 runs CALCREVISE for these). Bit 1: in the party (UNITECH@0x0031e sets it, LEAVECH@0x000a8 clears it, ADDEXP@0x00008 tests it). Bit 2: set for a kind-3 unit (UNITECH@0x002d3, UNSUMMON@0x00007), not decoded. |
| `0x8b227` | lead_saved_pool_rate | med | investigation | Saved: the copy of lead_pool_rates for the current form (CHANGECHAPTER@0x0012c). |
| `0x8b228` | lead_parameters | high | investigation | Saved: セリカ's seven parameters (labels on CHMODE2@0x01adf to 0x01b39). CALCCHP computes his level G[0x8b2ff] = (sum - 1) / 14 + 1 and his HP, MP and TP from them. Columns: 0=strength, 1=vitality, 2=agility, 3=dexterity, 4=intelligence, 5=spirit, 6=charm. |
| `0x8b236` | character_unit_ids | high | investigation | Saved. CHINIT (at every boot, INIT@0x00000): the character's EBINIT unit (UNITECH@0x00001, CALCREVISE@0x00330, DRAWCHP@0x0012f and 339 more). CHANGECHAPTER@0x00040 sets セリカ's (character 1) to his form G[0x30b1]; a load overwrites CHINIT's values, so a change reaches only new games. |
| `0x8b2fe` | character_levels | high | investigation | Saved: the level; UNITECH@0x0008d sets it to unit_base_levels; CALCLVUP and CALCNEXTEXP@0x0000c read it. |
| `0x8b362` | character_experience | high | investigation | Saved: the experience; UNITECH@0x00099 zeroes it, ADDEXP@0x00027 adds to it. |
| `0x8b42a` | character_hp | high | investigation | Saved: current HP in points, capped by the derived max (CALCREVISE@0x0013d). |
| `0x8b48e` | character_max_hp | high | investigation | Saved: max HP x100 before item bonuses (CALCREVISE). |
| `0x8b4f2` | character_mp | med | investigation | Saved: current MP in points (CALCREVISE). |
| `0x8b556` | character_max_mp | high | investigation | Saved: max MP x100 before item bonuses (CALCREVISE). |
| `0x8b5ba` | character_tp | med | investigation | Saved: current TP in points (CALCREVISE). |
| `0x8b61e` | character_max_tp | high | investigation | Saved: max TP x100 before item bonuses (CALCREVISE). |
| `0x8b682` | character_attacks | high | investigation | Saved: 攻撃力 x100; UNITECH copies unit_attacks, CALCLVUP@0x00088 adds unit_attack_growth per level gained. |
| `0x8b6e6` | character_defenses | high | investigation | Saved: 防御力 x100; UNITECH copies unit_defenses, CALCLVUP@0x000ab adds unit_defense_growth per level gained. |
| `0x8b74a` | character_magic_attacks | high | investigation | Saved: 魔法攻撃 x100; UNITECH copies unit_magic_attacks, CALCLVUP@0x000ce adds unit_magic_attack_growth per level gained. |
| `0x8b7ae` | character_magic_defenses | high | investigation | Saved: 魔法防御 x100; UNITECH copies unit_magic_defenses, CALCLVUP@0x000f1 adds unit_magic_defense_growth per level gained. |
| `0x8b812` | character_attack_counts | high | investigation | Saved: 攻撃回数 x100; UNITECH copies unit_attack_counts, CALCLVUP@0x00114 adds unit_attack_count_growth per level gained. |
| `0x8b876` | character_defense_counts | high | investigation | Saved: 防御回数 x100; UNITECH copies unit_defense_counts, CALCLVUP@0x00137 adds unit_defense_count_growth per level gained. |
| `0x8b8da` | character_physical_resistances | med | investigation | Saved: 肉体抵抗 x100; UNITECH copies unit_physical_resistances, CALCLVUP@0x0015a adds unit_physical_resistance_growth per level gained. |
| `0x8b93e` | character_mental_resistances | med | investigation | Saved: 精神抵抗 x100; UNITECH copies unit_mental_resistances, CALCLVUP@0x0017d adds unit_mental_resistance_growth per level gained. |
| `0x8b9a2` | character_physical_speeds | med | investigation | Saved: 肉体戦速 x100; UNITECH copies unit_physical_speeds, CALCLVUP@0x001a0 adds unit_physical_speed_growth per level gained. |
| `0x8ba06` | character_mental_speeds | med | investigation | Saved: 精神戦速 x100; UNITECH copies unit_mental_speeds, CALCLVUP@0x001c3 adds unit_mental_speed_growth per level gained. |
| `0x8ba6a` | character_luck | low | investigation | Saved: 運 x100; UNITECH copies unit_luck, CALCLVUP@0x001e6 adds unit_luck_growth per level gained. |
| `0x8bace` | character_skill_ranks | high | investigation | Saved: the rank in each of the unit's skill categories (unit_skill_categories); UNITECH@0x001e9 sets it, BTL@0x06ebc raises it. |
| `0x8be52` | character_skill_experience | med | investigation | Saved: the experience toward the next rank in each category; BTL@0x02bee adds to it, BTL@0x06ee4 zeroes it on a rank-up. |
| `0x8c1d6` | character_weapons | high | investigation | Saved: the equipped weapon; SETCH@0x001f1 puts it in the slot's G[0x38962f]. |
| `0x8c302` | character_shields | high | investigation | Saved: the equipped shield; SETCH@0x00227 puts it in G[0x38964d]. |
| `0x8c366` | character_armors | high | investigation | Saved: the equipped armor; SETCH@0x0020c puts it in G[0x38966b]. |
| `0x8c3ca` | character_accessories | high | investigation | Saved: the equipped accessory; SETCH@0x00242 puts it in G[0x389689]. |
| `0x8e36e` | character_tactics | med | investigation | Saved: the chosen tactic, a column of unit_tactic_routines (BTL@0x013e6; CHMODE2@0x0225f marks it). |
| `0x8f56d` | scene_seen_flags | high | investigation | SYSTEM4@0x2f9..0x336 sets [current_scene_id] to 1 when a story scene plays (scene_run_mode 0, ids 0 to 1999) and stores the cell in the shared profile; the map events' inline pass does the same (MPEV01B0102@0x254). INFO1@0x902 loads them to unlock glossary entries; TITLE's inline scene pass sets them the same way (TITLE@0x775). |
| `0x8fd3d` | speaker_heard_flags | high | investigation | Kept in the shared profile: 1 once the speaker (a CNINIT id, the same number as its unit) has talked. Every spoken page sets it and stores it (SC0470@0x00b62..0x00b75); the 設定 voice page shows a setting's row once its unit is heard (CONFIG@0x010e7), and the 人物 book names a profile so (INFO2@0x00876). |
| `0x91027` | scene_graphic_element_numbers | med | investigation | INIT2@0x4c..0x6f: [n] = 10000 + 10n for layers 0, 1 and 11 to 16; [2..10] have no writer and no reader. A scene sets the layer in G[0x91022] and transforms element [layer] (SC0000@0xfee1..0xff3c). |
| `0x91059` | speaker_voice_units | high | investigation | CNINIT: the unit whose voice setting the speaker uses (SC0470@0x00bca to 0x00bdf); battle reads it by unit (BTL@0x019aa, ANIME@0x00cf0, CHANGECH@0x00b58, RESULT@0x01134). |
| `0x91441` | unit_voice_settings | high | investigation | CVINIT, by unit: the voice setting of a voice unit (a speaker_voice_units value). SC0470@0x00bca mutes a line when G[0x2be7 + this] is set; BTL@0x019aa likewise. |
| `0x91829` | voice_setting_units | high | investigation | CVINIT: the unit whose heard flag G[0x8fd3d + unit] shows the row (CONFIG@0x010e7); setting 0 has none. |
| `0x91847` | voice_setting_samples | high | investigation | CVINIT: the sample voice clip the 設定 voice page plays (CONFIG@0x00e90); the table's leader. |
| `0x9186b` | cg_pictures | high | investigation | CGINIT's leader. 0 the CG (CGMODE@0x9a7, @0xb7f; $1$CGINIT adds 712..715). 1, on 338, a 160 x 120 picture (EVM008G.AGF ...) that ALLMAP@0x2aa..0x2fc and SAVE@0xb04..0xb2f draw as a save slot's thumbnail when the screen shows picture 0. Columns: 0=picture, 1=save_thumbnail. |
| `0x9280b` | cg_tabs | high | investigation | CGINIT: the CGMODE tab, 1 to 5 (CGMODE@0x959 takes 1 off it). |
| `0x92fdb` | cg_slots | high | investigation | CGINIT: the slot on the tab, 1 to 30 (CGMODE@0x967). |
| `0x937ab` | cg_variants | high | investigation | CGINIT: the variant within the slot, 1 to 30. CGMODE@0x94b skips a CG whose variant is 0 (154, 155, 156, 254, 293 and 379 have none) and @0x983..0x9a0 counts each slot's variants. |
| `0x93f7b` | cg_scroll_x | med | investigation | CGINIT, on 31 CGs: the x scroll of a picture larger than the screen; CGMODE@0xbcc places it at x = -value. |
| `0x9474b` | cg_scroll_y | med | investigation | CGINIT, on 77 CGs: the y scroll, the same way (CGMODE@0xbdf). |
| `0x94f1b` | replay_scripts | high | investigation | SPINIT, by page x 15 + cell: the scene to replay, a script id (SP0003.BIN ... SP0582.BIN, 41 of them). HMODE@0x8b9 walks 5 pages of 15, counts a cell unlocked when its script is catalog-unlocked (HMODE@0x8dd), and HMODE@0xb44 runs the chosen one. HMODE turns pages modulo 3 (HMODE@0x84c). |
| `0x95376` | lead_strength_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 筋力; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 0 to it at a chapter change. |
| `0x95380` | lead_strength_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 筋力 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x9538a` | lead_vitality_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 体力; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 1 to it at a chapter change. |
| `0x95394` | lead_vitality_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 体力 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x9539e` | lead_agility_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 敏捷性; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 2 to it at a chapter change. |
| `0x953a8` | lead_agility_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 敏捷性 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x953b2` | lead_dexterity_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 器用度; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 3 to it at a chapter change. |
| `0x953bc` | lead_dexterity_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 器用度 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x953c6` | lead_intelligence_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 知性; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 4 to it at a chapter change. |
| `0x953d0` | lead_intelligence_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 知性 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x953da` | lead_spirit_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 精神力; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 5 to it at a chapter change. |
| `0x953e4` | lead_spirit_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 精神力 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x953ee` | lead_charm_minimums | high | investigation | CBINIT, by セリカ's unit lead_unit_id: the lowest 魅力; CHANGECHAPTER@0x00045 to 0x00127 raises lead_parameters column 6 to it at a chapter change. |
| `0x953f8` | lead_charm_cost_rates | med | investigation | CBINIT, by form: a percentage on the cost of raising 魅力 (CHMODE2@0x00298 to 0x002e0, used at CHMODE2@0x01758). |
| `0x95402` | lead_pool_rates | high | investigation | CBINIT, by form: a percentage on セリカ's HP, MP and TP (CALCCHP@0x00244, @0x00298, @0x002d7); CHANGECHAPTER@0x0012c copies it into lead_saved_pool_rate. |
| `0x9540d` | unit_voice_clips | med | investigation | EBINIT: the unit's voice clips, columns 0 to 26 written. BTL@0x019d2 plays column 17, BTL@0x03bf0 column 21, CHANGECH@0x00b80 column 20; BTL@0x03863 plays column 18 when a slot leaves the battle (BTL@0x036ef). BTL@0x019aa mutes a clip when G[0x2be7 + unit_voice_settings[speaker_voice_units[unit]]] is set. The init_layout row G[0x95402] stride 30 is this array seen 11 cells early. Columns: 18=fall. |
| `0x9c93d` | unit_pictures | med | investigation | EBINIT: three pictures. Column 0 is the bestiary portrait (INFO3@0x00bfb, surface 0x7f) and a battle picture (BTL@0x06069, surface 0x30); column 1 BTL@0x05f6b (surface 0x2f). Columns 1 and 2 not decoded. Columns: 0=portrait. |
| `0x9d4f5` | unit_battle_sprites | high | investigation | EBINIT: battle sprite sheets. BTL@0x01cda loads column 0 into surface 0x34 + slot; BTL@0x01610, @0x016e4, @0x01723 read columns 3, 5, 7; DRAWCHP@0x00177 draws column 9 instead of 0 when the character's G[0x8c42e + 40 x character + 3] is set. Only the 26 party units fill columns 1 to 9. |
| `0x9fc05` | unit_status_portraits | high | investigation | EBINIT: the status screen's portrait of a party unit (DRAWCHP@0x00150, surface 0x85). |
| `0x9ffed` | unit_book_sort_keys | high | investigation | EBINIT: the bestiary's order; INFO3@0x00706 sorts by it and INFO3@0x00738 lists only units above 0. |
| `0xa03d5` | unit_fs_temperaments | med | investigation | EBINIT: how the unit's FS (戦意) moves. CALCDAMAGE@0x009a6: 1 gives an FS change G[0x952aa] of -2, 2 gives -1, others +1; BTL@0x0250b to 0x0266a scales other FS changes by it. Every unit has 3. |
| `0xa07bd` | unit_kinds | med | investigation | EBINIT: 2 for a party human, 3 for a summoned partner, 1 for an enemy; 0, where EBINIT writes no kind, for a name with no battle body (a speaker: 村人, 男の声), 100 of the 102 with no stats. A party unit's battle sprite is a strip of six frames, 720 x 120, an enemy's one picture. CHANGECHAPTER@0x00466 and LEAVECH@0x00142 run UNSUMMON for 3; UNITECH@0x002b7 sets character_flags bit 2 for 3; CHMODE1, CHMODE2, CHMODE3 and CHANGECH@0x00c9c pick a picture at (kind - 2) x 45. Values: 1 Enemy, 2 Party member, 3 Summoned partner. |
| `0xa0ba5` | unit_boss_classes | med | investigation | EBINIT: 1 to 5 for a boss. BTL@0x00100 to 0x0017b plays battle music 0x17 + the highest class among the enemy slots; CALCILL@0x0030b makes actions with G[0x23baad] = 1 skip a target with a class. |
| `0xa0f8d` | unit_sizes | med | investigation | EBINIT: the unit's size, 1 to 5. SETENP@0x004ca places it on a 5-wide grid; SELTARGET@0x00b33 and TACTICS@0x00c9a scale its sprite by it; INFO3@0x00bce moves the bestiary sprite 24 down per size. |
| `0xa175d` | unit_defense_elements | high | investigation | EBINIT: the defense element, shown as GS[0x8028 + element] (INFO3@0x01156); SETEN@0x0082b copies it into the slot's G[0x389701] when the armor gives none. |
| `0xa1b45` | unit_initial_weapons | high | investigation | EBINIT: the starting weapon, an item id. SETEN@0x0049b puts it in the slot's G[0x38962f]; UNITECH@0x0026f copies it into character_weapons. |
| `0xa1f2d` | unit_initial_shields | high | investigation | EBINIT: the starting shield, an item id (2801 小さな円形の盾). SETEN@0x004b3 puts it in G[0x38964d]; UNITECH@0x00282 copies it into character_shields. |
| `0xa2315` | unit_initial_armors | high | investigation | EBINIT: the starting armor, an item id. SETEN@0x004a7 puts it in G[0x38966b]; UNITECH@0x00295 copies it into character_armors. |
| `0xa26fd` | unit_initial_accessories | high | investigation | EBINIT: the starting accessory, an item id. SETEN@0x004bf puts it in G[0x389689]; UNITECH@0x002a8 copies it into character_accessories. |
| `0xa2ae5` | unit_equip_kind_flags | med | investigation | EBINIT: the item kinds the unit may equip, bit G[0x12cabd + item] (CHMODE1@0x018ce, SHOP@0x03a1c). |
| `0xa2ecd` | unit_skill_categories | high | investigation | EBINIT: the unit's skill categories, item category ids named by GS[0x803c + id] (小型武器, 魔術・電撃, 必殺・飛燕). ADDSKILL@0x00031, CHECKACT@0x00069, CHMODE2@0x014d1 and BTL@0x02b77 match a skill's category G[0x12f1cd] against them. |
| `0xa51f5` | unit_skill_category_open_flags | high | investigation | EBINIT: 1 when the category is open as the character joins; UNITECH@0x001cc then copies unit_skill_start_ranks, and UNITECH@0x0022c clears the rank of a category with 0. |
| `0xa751d` | unit_skill_start_ranks | high | investigation | EBINIT: the category's starting rank; UNITECH@0x001de copies it into character_skill_ranks. |
| `0xa9845` | unit_skill_max_ranks | med | investigation | EBINIT: the category's highest rank; BTL@0x06e25 raises character_skill_ranks only below it, with a 12/10 threshold while it is below 7 (BTL@0x06e87). |
| `0xabb6d` | unit_support_effects | high | investigation | EBINIT: the support effect (支援効果) the unit gives, named by GS[0x7ee8 + id] (CHMODE2@0x01205, @0x02651); BTL@0x04bed counts the party's effects in G[0x95311 + id]. |
| `0xad2dd` | unit_base_levels | high | investigation | EBINIT: the level of the base stats (SETEN@0x00178, @0x001c0); UNITECH@0x0008d makes it a joining character's level. |
| `0xad6c5` | unit_max_levels | med | investigation | EBINIT: the highest level SETEN gives the unit as an enemy (SETEN@0x00157). |
| `0xadaad` | unit_level_catchup_divisors | med | investigation | EBINIT: the divisor of an enemy's level catch-up toward the party (SETEN@0x00068 to 0x00107). |
| `0xade95` | unit_starting_fs | high | investigation | EBINIT: an enemy slot's starting FS (SETEN@0x00a49, into G[0x3893f5]); the bestiary shows it (INFO3@0x00d06). |
| `0xae27d` | unit_max_fs | high | investigation | EBINIT: the maximum FS, at least 2. SETEN@0x001da (into G[0x389413]), SETCH@0x00047 and CALCREVISE@0x00337 read it live. |
| `0xae665` | unit_max_hp | high | investigation | EBINIT: max HP x100 at unit_base_levels. SETEN@0x001e6 sets the slot's G[0x38944f] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b48e]. CALCREVISE adds the items' bonus. |
| `0xaea4d` | unit_max_mp | high | investigation | EBINIT: max MP x100 at unit_base_levels. SETEN@0x00210 sets the slot's G[0x38948b] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b556]. CALCREVISE adds the items' bonus. |
| `0xaee35` | unit_max_tp | high | investigation | EBINIT: max TP x100 at unit_base_levels. SETEN@0x0023a sets the slot's G[0x3894c7] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b61e]. CALCREVISE adds the items' bonus. |
| `0xaf21d` | unit_attacks | high | investigation | EBINIT: 攻撃力 x100 at unit_base_levels. SETEN@0x00264 sets the slot's G[0x3894e5] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b682]. 攻撃力 on INFO3@0x0108a. |
| `0xaf605` | unit_defenses | high | investigation | EBINIT: 防御力 x100 at unit_base_levels. SETEN@0x0028e sets the slot's G[0x389503] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b6e6]. 防御力 on INFO3@0x010c0. |
| `0xaf9ed` | unit_magic_attacks | high | investigation | EBINIT: 魔法攻撃 x100 at unit_base_levels. SETEN@0x0030c sets the slot's G[0x389521] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b74a]. 魔法攻撃 on INFO3@0x010f6. |
| `0xafdd5` | unit_magic_defenses | high | investigation | EBINIT: 魔法防御 x100 at unit_base_levels. SETEN@0x00336 sets the slot's G[0x38953f] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b7ae]. 魔法防御 on INFO3@0x01111. |
| `0xb01bd` | unit_attack_counts | high | investigation | EBINIT: 攻撃回数 x100 at unit_base_levels. SETEN@0x002b8 sets the slot's G[0x38955d] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b812]. 攻撃回数 on INFO3@0x010a5. |
| `0xb05a5` | unit_defense_counts | high | investigation | EBINIT: 防御回数 x100 at unit_base_levels. SETEN@0x002e2 sets the slot's G[0x38957b] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b876]. 防御回数 on INFO3@0x010db. |
| `0xb098d` | unit_physical_resistances | med | investigation | EBINIT: 肉体抵抗 x100 at unit_base_levels. SETEN@0x00360 sets the slot's G[0x389599] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b8da]. 地脈の衣 '肉体抵抗が高い' raises it (G[0x3260ad]); CALCILL@0x0038d. |
| `0xb0d75` | unit_mental_resistances | med | investigation | EBINIT: 精神抵抗 x100 at unit_base_levels. SETEN@0x0038a sets the slot's G[0x3895b7] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b93e]. 魔術師のローブ '精神抵抗+2' raises it (G[0x3287bd]); CALCILL@0x0034b. |
| `0xb115d` | unit_physical_speeds | med | investigation | EBINIT: 肉体戦速 x100 at unit_base_levels. SETEN@0x003b4 sets the slot's G[0x3895d5] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8b9a2]. 剣の耳飾り '肉体戦速+1' raises it (G[0x32aecd]); BTL@0x017dd. |
| `0xb1545` | unit_mental_speeds | med | investigation | EBINIT: 精神戦速 x100 at unit_base_levels. SETEN@0x003de sets the slot's G[0x3895f3] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8ba06]. 杖の耳飾り '精神戦速+1' raises it (G[0x32d5dd]); BTL@0x017c7. |
| `0xb192d` | unit_luck | low | investigation | EBINIT: 運 x100 at unit_base_levels. SETEN@0x00408 sets the slot's G[0x389611] to (base + growth x (level - base level)) / 100; UNITECH copies it into G[0x8ba6a]. a hit-chance term in CALCDAMAGE@0x002b6; 神刀フォルトゥナ gives 20. |
| `0xb1d15` | unit_max_hp_growth | high | investigation | EBINIT: max HP growth x100 per level; SETEN@0x001e6 uses it, and CALCLVUP@0x0001f adds it to the saved G[0x8b48e] for each level gained. |
| `0xb20fd` | unit_max_mp_growth | high | investigation | EBINIT: max MP growth x100 per level; SETEN@0x00210 uses it, and CALCLVUP@0x00042 adds it to the saved G[0x8b556] for each level gained. |
| `0xb24e5` | unit_max_tp_growth | high | investigation | EBINIT: max TP growth x100 per level; SETEN@0x0023a uses it, and CALCLVUP@0x00065 adds it to the saved G[0x8b61e] for each level gained. |
| `0xb28cd` | unit_attack_growth | high | investigation | EBINIT: 攻撃力 growth x100 per level; SETEN@0x00264 uses it, and CALCLVUP@0x00088 adds it to the saved G[0x8b682] for each level gained. |
| `0xb2cb5` | unit_defense_growth | high | investigation | EBINIT: 防御力 growth x100 per level; SETEN@0x0028e uses it, and CALCLVUP@0x000ab adds it to the saved G[0x8b6e6] for each level gained. |
| `0xb309d` | unit_magic_attack_growth | high | investigation | EBINIT: 魔法攻撃 growth x100 per level; SETEN@0x0030c uses it, and CALCLVUP@0x000ce adds it to the saved G[0x8b74a] for each level gained. |
| `0xb3485` | unit_magic_defense_growth | high | investigation | EBINIT: 魔法防御 growth x100 per level; SETEN@0x00336 uses it, and CALCLVUP@0x000f1 adds it to the saved G[0x8b7ae] for each level gained. |
| `0xb386d` | unit_attack_count_growth | high | investigation | EBINIT: 攻撃回数 growth x100 per level; SETEN@0x002b8 uses it, and CALCLVUP@0x00114 adds it to the saved G[0x8b812] for each level gained. |
| `0xb3c55` | unit_defense_count_growth | high | investigation | EBINIT: 防御回数 growth x100 per level; SETEN@0x002e2 uses it, and CALCLVUP@0x00137 adds it to the saved G[0x8b876] for each level gained. |
| `0xb403d` | unit_physical_resistance_growth | med | investigation | EBINIT: 肉体抵抗 growth x100 per level; SETEN@0x00360 uses it, and CALCLVUP@0x0015a adds it to the saved G[0x8b8da] for each level gained. |
| `0xb4425` | unit_mental_resistance_growth | med | investigation | EBINIT: 精神抵抗 growth x100 per level; SETEN@0x0038a uses it, and CALCLVUP@0x0017d adds it to the saved G[0x8b93e] for each level gained. |
| `0xb480d` | unit_physical_speed_growth | med | investigation | EBINIT: 肉体戦速 growth x100 per level; SETEN@0x003b4 uses it, and CALCLVUP@0x001a0 adds it to the saved G[0x8b9a2] for each level gained. |
| `0xb4bf5` | unit_mental_speed_growth | med | investigation | EBINIT: 精神戦速 growth x100 per level; SETEN@0x003de uses it, and CALCLVUP@0x001c3 adds it to the saved G[0x8ba06] for each level gained. |
| `0xb4fdd` | unit_luck_growth | low | investigation | EBINIT: 運 growth x100 per level; SETEN@0x00408 uses it, and CALCLVUP@0x001e6 adds it to the saved G[0x8ba6a] for each level gained. |
| `0xb53c5` | unit_tactic_routines | med | investigation | EBINIT: the AI routine for each tactic (作戦), column k for tactic k named GS[0x812f + k + 1] (通常攻撃型 to ランダム型). CHMODE2@0x011d8 lists the set ones; BTL@0x01401 runs RTN with the column of character_tactics (BTL@0x013e6). Enemies fill column 0 only. |
| `0xb76ed` | unit_action_ids | med | investigation | EBINIT: the unit's six actions (item and skill ids); RTN@0x0008b copies them into G[0x3866a7], putting the slot's weapon (or 9999) first when the first is 0. |
| `0xb8e5d` | unit_exp_curves | high | investigation | EBINIT: the experience curve; CALCNEXTEXP@0x0002b gives 200, 215, 225, 235, 250 per level for 1 to 5 and no next level otherwise. |
| `0xb9245` | unit_exp_rates | high | investigation | EBINIT: a percentage on the next level's experience, 0 for 100 (CALCNEXTEXP@0x000d9). The next level costs curve x level x 124 / 1000 x rate / 100, at least 1. |
| `0xb962d` | unit_exp_rewards | med | investigation | EBINIT: the experience the battle's pool G[0x952ac] gets from the unit as it leaves the battle: column 0 when its HP G[0x389431] is 0, column 1 otherwise (BTL@0x0374c, @0x0379e, @0x03835). RESULT@0x0068a passes each character's pool to ADDEXP. Columns: 0=defeated, 1=not_defeated. |
| `0xb9dfd` | unit_money | high | investigation | EBINIT: the money the unit gives; RESULT@0x00433 sums it over the defeated slots and RESULT@0x011cf adds it to 所持金 G[0x8b159]. |
| `0xba1e5` | unit_drop_chances | high | investigation | EBINIT: each drop's chance in percent; RESULT@0x0049b takes the first drop whose chance beats a draw of 0 to 99. |
| `0xbb56d` | unit_drop_items | high | investigation | EBINIT: the drop items (item ids); RESULT@0x00489, @0x00509, given by ADDITEM at RESULT@0x011c9. |
| `0xbc8f6` | scene_script_ids | high | investigation | SCINIT: the script that plays scene [id], a packed script id (0x28 SC0000.BIN; an append's scene carries selector 1 in the top byte, 0x1000183 $1$SC0000.BIN). SYSTEM4@0x46d copies it into current_scene_script_id and runs it; MENU@0x733 runs it directly. $1$SCINIT, run in 特別編 only, gives scenes 0, 1, 10..13, 20..23, 30..34, 40..44, 50..54, 60..63 and 100 to append scripts. |
| `0xbf006` | scene_offer_contexts | med | investigation | SCINIT's first pass (SCINIT@0x0..0x776) writes the SCJUMP context that offers the scene, 1, 2, 3, 5 or 6 on 164 scenes; they agree with SCJUMP's rules except 395 and 1510, which no rule names. The second pass (SCINIT@0x77b..) sets -1 on 669 scenes, so 32 keep a context. Each scene's header (SC0000@0x15f) registers the right-click menu unless in_room is set or the value is 4, which no scene has. |
| `0xc1716` | area_pictures | high | investigation | AMINIT: 0 the icon ALT0nnA.AGF, texture 0x3e + n, drawn 120x120 centred on the point (ALLMAP@0x9b3, 0xa30; SHOWALLMAP@0xf4); 1 the 200x200 panel ALS0nnA.AGF for the area under the cursor, texture 0x3d (ALLMAP@0xb1e..0xb8e), and the highlight of SHOWALLMAP@0xb2; 2 a town's full-screen backdrop MT0nnA.AGF, texture 0x3d (TOWN@0x5ee, 0x691), written for towns only. Columns: 0=icon, 1=panel, 2=town_backdrop. |
| `0xc1842` | area_town_music | high | investigation | AMINIT: a town's BGM number, 2 to 18, played by TOWN@0x188. AMINIT writes it for towns but never clears it. |
| `0xc1a36` | area_kinds | high | investigation | AMINIT: what entering does: 1 FIELD (system mode 3), 2 TOWN (system mode 5), 0 nothing, the junction 90 中継点 (ALLMAP@0x752..0x78c). |
| `0xc1a9a` | area_x | high | investigation | AMINIT: the x of the area's point on the world map; the hot rectangle is 40 to each side (ALLMAP@0x118, SHOWALLMAP@0x5a). |
| `0xc1afe` | area_y | high | investigation | AMINIT: the y of the area's point; the hot rectangle is 40 above and below (ALLMAP@0x12b, SHOWALLMAP@0x6d). |
| `0xc1b62` | area_up_neighbors | high | investigation | AMINIT: the area the cursor moves to on up (joy 0), passing through areas whose state is 0 (ALLMAP@0x84f, 0x5db..0x6f7). |
| `0xc1bc6` | area_down_neighbors | high | investigation | AMINIT: the area the cursor moves to on down (joy 2), passing through hidden areas (ALLMAP@0x895). |
| `0xc1c2a` | area_left_neighbors | high | investigation | AMINIT: the area the cursor moves to on left (joy 3), passing through hidden areas (ALLMAP@0x8db). |
| `0xc1c8e` | area_right_neighbors | high | investigation | AMINIT: the area the cursor moves to on right (joy 1), passing through hidden areas (ALLMAP@0x921). |
| `0xc1cf2` | town_site_pictures | high | investigation | SIINIT: column 1 is the site's picture MTS0nnX.AGF on texture 0x3e + n (TOWN@0x630): its top half drawn normally, its bottom half when selected (TOWN@0x6f0, 0x7ae). Column 0 is never written. Columns: 1=picture. |
| `0xc1d2e` | town_site_x | high | investigation | SIINIT: the x where the site's picture is drawn (TOWN@0x141, 0x6e2). |
| `0xc1d4c` | town_site_y | high | investigation | SIINIT: the y where the site's picture is drawn (TOWN@0x154, 0x6e2). |
| `0xc1d6a` | town_site_hot_lefts | high | investigation | SIINIT: the hot area's left, relative to the site's x (TOWN@0xb9..0x133; find-hit-rectangle at TOWN@0x268 subtracts it). |
| `0xc1d88` | town_site_hot_tops | high | investigation | SIINIT: the hot area's top, relative to the site's y (TOWN@0xb9..0x133). |
| `0xc1da6` | town_site_hot_widths | high | investigation | SIINIT: the hot area's width (TOWN@0xb9..0x133). |
| `0xc1dc4` | town_site_hot_heights | high | investigation | SIINIT: the hot area's height (TOWN@0xb9..0x133). |
| `0xc1de2` | town_site_neighbors | high | investigation | SIINIT: the site the cursor moves to on up (joy 0, TOWN@0x395), down (joy 2, @0x3e8), left (joy 3, @0x43b) and right (joy 1, @0x48e), passing through closed sites (TOWN@0x56c..0x5c3). Columns: 0=up, 1=down, 2=left, 3=right. |
| `0xc1e5c` | shop_sign | med | investigation | SHINIT: which of four 140x40 cells of the sign sheet (texture 0x7c) SHOP draws, 0..3 (SHOP@0x23b2..0x2434); item shops write 0, equipment shops 1. |
| `0xc1e5d` | shop_items | high | investigation | SHINIT: the item sold in slot 10 * page + slot (pages 0 and 1, slots 0..6), 0 for none, negated while its story flag is unset (SHINIT@0x51..0x66); SHOP@0x634 reads it, SHOP@0x27ab takes the absolute value. SHINIT@0x0 zeroes the 20 cells first. |
| `0xc1e71` | shop_keeper_picture | high | investigation | SHINIT: the shopkeeper's picture, a resource id (11884 CS136AA and others), loaded at SHOP@0x2316; SHOP@0x87 clears it before SHINIT. |
| `0xc1e72` | shop_background | high | investigation | SHINIT: the shop's background picture (11883 BG018A and others), loaded at SHOP@0x2308; 0 for shop 11. SHOP@0x82 clears it before SHINIT. |
| `0xc1e75` | arena_rank_pictures | high | investigation | ARINIT: the picture ARENA draws at the bottom, centred at x 670 (ARENA@0x98c..0x9a1, @0xc81..0xcb2); CS480AA.AGF for every rank. |
| `0xc1e79` | arena_rank_backdrops | high | investigation | ARINIT: the full-screen backdrop of ARENA (ARENA@0x970..0x985, @0xc5b..0xc70) and of CHMODE opened from it (CHMODE@0x15e); BG048A.AGF for every rank. |
| `0xc1e7d` | arena_round_marks | med | investigation | ARINIT: nonzero lists the round (ARENA@0xe21, @0x1243); the value picks a 12 by 14 glyph of SO016.AGF beside it (ARENA@0xe59..0xe72). It is the round number plus one in every record. |
| `0xc1ea5` | arena_round_enemy_levels | high | investigation | ARINIT: the enemies' level, which SETEN@0x45 takes while ARENA runs (G[0x91054]). |
| `0xc1ecd` | arena_round_formations | high | investigation | ARINIT: the round's two formations (ESINIT ids), into G[0x9529b] and G[0x9529c] (ARENA@0x708..0x756); column 1 is never written. Columns: 0=first, 1=second. |
| `0xc1f1d` | arena_round_fees | high | investigation | ARINIT: the fee (掛金), drawn at ARENA@0xf99; the round is refused when the money G[0x8b159] does not cover it, and it comes off the money (ARENA@0x691..0x6e5). |
| `0xc1f45` | arena_round_prizes | high | investigation | ARINIT: the prize money (賞金), drawn at ARENA@0x1316; RESULT pays it only on the round's first win (RESULT@0x2ef..0x312, added at @0x11cf). |
| `0xc1f6d` | arena_round_reward_items | high | investigation | ARINIT: two reward items (賞品), ITINIT ids, given through ADDITEM on the first win (RESULT@0x342..0x3cc, @0x11a3..0x11c9; drawn at ARENA@0x1373). |
| `0xc1fbd` | arena_round_reward_counts | high | investigation | ARINIT: how many of each reward item; an item with count 0 is not given (RESULT@0x362, ARENA@0x1353). |
| `0xc200d` | arena_round_required_levels | high | investigation | ARINIT: the round is closed while the party's average of G[0x8b2fe] is below it; 0 for none (ARENA@0x9a9..0xa1c, @0xa6c..0xa97). The condition text matches: 35 reads 必要平均LV35. |
| `0xc2035` | arena_round_party_limits | high | investigation | ARINIT: the round is closed while the party has more members; 0 for any (ARENA@0xa9f..0xaca). Only round 26 has one, 1 (１人専用). |
| `0xc205d` | arena_round_characters | high | investigation | ARINIT: five characters who must be in the party G[0x8b14c] (ARENA@0xad2..0xb29). Only round 26 names one, character 1 (セリカ, セリカ一人での限定バトル). |
| `0xc214d` | map_object_sheets | high | investigation | MPINIT: the map's object sheet MO01.AGF (MO18.AGF for map 18), set on texture 0x66 (FIELD@0x21f2); DRAWMAP@0x44c cuts every object sprite from it. |
| `0xc217f` | map_start_floors | high | investigation | MPINIT: the floor the map starts on, written to map_block_row on entry (FIELD@0xba..0xf1). |
| `0xc21b1` | map_start_sections | high | investigation | MPINIT: the section the map starts on, written to map_block_column on entry (FIELD@0xba..0xf1). |
| `0xc21e3` | map_start_x | high | investigation | MPINIT: the start x, written to map_player_target_x on entry (FIELD@0xba..0xf1). |
| `0xc2215` | map_start_y | high | investigation | MPINIT: the start y, written to map_player_target_y on entry (FIELD@0xba..0xf1). |
| `0xc2247` | map_start_facings | high | investigation | MPINIT: the start facing (0 to 7, as map_player_facing), written to map_player_target_facing on entry (FIELD@0xba..0xf1); four maps leave it 0. |
| `0xc2279` | map_encounter_decay_distances | high | investigation | MPINIT: while the field gauge G[0x3178] is above -15, it falls by 1 each time the distance G[0x3179] reaches ten times this value, which restarts the distance (FIELD@0x10ad..0x10ec). |
| `0xc22ab` | map_keeps_section_states | med | investigation | MPINIT: 1 on 13 maps. CHANGECHAPTER@0x4a4..0x576 clears all 500 saved section slots (map_section_codes and the per-slot object arrays) unless this cell is 1 for the map of the slot in play (map_section_codes[map_section_slot] / 10000). |
| `0xc22dd` | map_floor_numbers | high | investigation | MPINIT: each floor's number as its name shows it, columns 1 to 9, negative below ground. No script reads it. The array ends at 0xc3665, where RMINIT's begin. |
| `0xc3665` | map_block_scripts | high | investigation | RMINIT: the section's map script MPmmFrrss.BIN, called at FIELD@0x2c0. |
| `0xc3827` | map_block_event_scripts | high | investigation | RMINIT: the section's event script MPEVmmFrrss.BIN, called at FIELD@0xd2a. |
| `0xc39e9` | map_block_pictures | high | investigation | RMINIT: the section's picture A (MP01F0101A.AGF ...), loaded on raw surface 0x3b; its size becomes the map size G[0xc57b5], G[0xc57b6] (FIELD@0x2132). |
| `0xc3bab` | map_block_pictures_2 | high | investigation | RMINIT: the section's picture B, loaded on surface 0x3c (FIELD@0x2152), 0 for none; DRAWMAP@0x46f..0x484 draws an 800x600 window of it at the scroll as element 0x9c4. |
| `0xc3d6d` | map_block_overlay_pictures | med | investigation | RMINIT: SO026A.AGF (a 200x200 picture) or once SO026B.AGF, set on texture 0x3d (FIELD@0x217d); DRAWMAP@0x495..0x4fc draws it as element 0x1387 at the negative scroll, scaled to cover the map size. What it shows in play is not checked. |
| `0xc3f2f` | map_block_minimap_pictures | high | investigation | RMINIT: the section's minimap MPS...A.AGF, on texture 0x8e in the 200x200 surface 0x70 (FIELD@0x24a8..0x24d3). |
| `0xc40f1` | map_block_battle_backdrops | high | investigation | RMINIT: the battle backdrop MPBnnnX.AGF on texture 0x2e; 0 uses MPB001A.AGF (BTL@0x556c..0x5593). |
| `0xc4475` | map_block_music | high | investigation | RMINIT: the section's BGM number (3 to 35) for each of three variants map_music_variant, read at 90 * floor + 3 * section + variant; 0 fades the music out (FIELD@0x606..0x64c). |
| `0xc4b7d` | map_block_encounter_sets | high | investigation | RMINIT: the section's encounter set (an ENINIT id) for each of two variants map_encounter_variant, read at 60 * floor + 2 * section + variant and written to G[0x125333] (FIELD@0x5c7..0x5e0); map scripts read it too (MP02B0207@0x26b). |
| `0xc4f01` | map_block_enemy_levels | high | investigation | RMINIT: the enemies' base level; SETEN@0x58 adds part of the party's average-level lead (EBINIT's G[0xadaad]) and clamps it by G[0xad6c5] and G[0xad2dd]. |
| `0xc50c3` | map_block_enemy_level_spreads | high | investigation | RMINIT: the level spread v, 1 to 10; SETEN draws 0 to 2v and subtracts v (SETEN@0x10e..0x150). |
| `0xc5285` | map_block_encounter_levels | high | investigation | RMINIT: the encounter level, 2 to 9, unless land_encounter_levels replaces it; G[0x3181] is added and the result, clamped to 0 to 9, is the column of EEINIT's G[0x125559 + 10 * band + level] (FIELD@0x1515..0x1577). |
| `0xc5447` | field_sprite_walk_sheets | high | investigation | CPINIT: the walking sheet CPM00nX.AGF of chapter n, by land_sprite_columns (0 to 5, A to F), loaded on texture 0x67 (FIELD@0x1806, 0x2207; DRAWCH@0x36; DRAWJUMP@0x221). Chapters 1 to 4. |
| `0xc5573` | field_sprite_jump_sheets | high | investigation | CPINIT: the jumping sheet CPJ00nX.AGF of chapter n, by land sprite column (DRAWJUMP@0xe8). Chapters 1 to 4. |
| `0xc57bb` | party_name_x | med | investigation | INIT2's inline array 10, 10, 10, 230, 230: the x of party slot [i]'s name. $1$MPEV49B0102@0x253 draws GS[0x9fe + G[0x8b236 + G[0x8b14c + i]]] at (x + 16, y + 7) for each filled slot; only 特別編's map events ($1$MPEV49B0102, 0103, 0109) read it. |
| `0xc57c0` | party_name_y | med | investigation | INIT2's inline array 390, 455, 520, 455, 520: the y of party slot [i]'s name, beside party_name_x. $1$MPEV49B0102@0x1e8 also fills the panel from y = [0]. |
| `0xd4d3d` | treasure_object_ids | high | investigation | The treasure of each object of the map in play: about 370 map scripts write it (MP03F0101@0x1f4: 121), FIELD clears it, and GETTREASURE@0x0 reads it at G[0xc57ac]. |
| `0xd9c8e` | land_footstep_sounds | high | investigation | LAINIT: the footstep loop on this land (WALKB.WAV, WALKC.WAV, B0208.WAV, B0016.WAV), looped on channel 3; 0 plays WALKA.WAV (FIELD@0x181f..0x1841, DRAWWALK@0x1a6). |
| `0xd9cf2` | land_sprite_columns | high | investigation | LAINIT: the column of field_sprite_walk_sheets (1 to 5, B to F) loaded on texture 0x67 when the land changes (FIELD@0x17d2..0x1811, DRAWCH@0xe, DRAWJUMP@0xdc); 0 is column A. |
| `0xd9d56` | land_speed_changes | high | investigation | LAINIT: the speed change in percent, -50 or -30: the step is max(1, 60 * (100 + v) / 1000) (FIELD@0x1795..0x17cb, DRAWWALK@0x135). |
| `0xd9dba` | land_drift_x | high | investigation | LAINIT: a current's x: each frame adds v * local 0x28b / 100 to the step (FIELD@0x1a4d..0x1a62, 0x1ec2). Lands 7 to 14 and 20 are currents. |
| `0xd9e1e` | land_drift_y | high | investigation | LAINIT: a current's y, added in the same way (FIELD@0x1a69..0x1a7e, 0x1ede). |
| `0xd9e82` | land_drift_walkable | high | investigation | LAINIT: 1 lets the player walk against the drift; with 0, FIELD drops the part of the step that opposes it (FIELD@0x19f6..0x1a48, 0x1e6b). Every land with a drift has 1. |
| `0xd9fae` | land_encounter_levels | high | investigation | LAINIT: an encounter level that replaces map_block_encounter_levels on this land (FIELD@0x1515); 9 on lands 19 and 21. |
| `0xda012` | object_kind_sprite_x | high | investigation | OBINIT: the sprite's x on the map's object sheet, texture 0x66 (DRAWMAP@0x40c). |
| `0xda3fa` | object_kind_sprite_y | high | investigation | OBINIT: the sprite's y on the object sheet (DRAWMAP@0x413). |
| `0xda7e2` | object_kind_widths | high | investigation | OBINIT: the sprite's width (DRAWMAP@0x41a). |
| `0xdabca` | object_kind_heights | high | investigation | OBINIT: the sprite's height (DRAWMAP@0x421). |
| `0xdafb2` | object_kind_depth_offsets | high | investigation | OBINIT2: added to the object's y to give its sort key (DRAWMAP@0x7f..0xc0); -9999 and 9999 put it at an end of the order. |
| `0xdb39a` | object_kind_wall_point_counts | high | investigation | OBINIT2: the number of points of the kind's wall polygon (5 or 7), written to G[0xc597d + poly] before AGERC's _SetMapWall* calls (FIELD@0xefa..0xfe6). |
| `0xdb782` | object_kind_wall_point_x | high | investigation | OBINIT2: the wall points' x, relative to the object; FIELD@0xefa..0xfe6 adds the object's x and writes it to G[0xc5a45 + 100 * poly + i]. |
| `0xf3e22` | object_kind_wall_point_y | high | investigation | OBINIT2: the wall points' y, relative to the object; written in the same way to G[0xca865 + 100 * poly + i]. |
| `0x10c4c2` | object_kind_next_kinds | high | investigation | OBINIT: the kind the object turns into when its switch G[0xd4829 + k] is above 0 and G[0x83c1a + switch] is set; -1 removes the object (EVOLVEOBJ@0x1d..0xe3). Map scripts read it too (67 scripts). |
| `0x10c8aa` | object_kind_change_sounds | high | investigation | OBINIT: the sound played when the object turns (SE001.WAV, B0401.WAV, B0302.WAV), when map_continue is not 0 (EVOLVEOBJ@0x1d..0xe3). |
| `0x10cc93` | treasure_items | high | investigation | TRINIT: the item in each of five slots, 0 or unwritten for a money slot; GETITEM@0x109..0x129 calls ADDITEM for each slot with a count. |
| `0x118fe3` | treasure_counts | high | investigation | TRINIT: how many of the slot's item, or the money for a money slot (10..36000; GETITEM@0x12f adds it to money). GETITEM@0x1d..0x7c skips a slot whose count is 0. |
| `0x125334` | encounter_entry_weights | high | investigation | ENINIT: the entry's weight in SETENP's draw over entries 1 to 99 (SETENP@0x1ec..0x2e3); entry 0 is never drawn but is the fallback when none fits, and FIELD@0x5ef runs the base ENINIT only while its weight is 0. |
| `0x125398` | encounter_entry_gauge_lows | high | investigation | ENINIT: the low end of the entry's window, plus 16: it may be drawn when the field gauge G[0x3178] >= value - 16; 0 for always (SETENP@0x217..0x233). |
| `0x1253fc` | encounter_entry_gauge_highs | high | investigation | ENINIT: the high end of the entry's window, plus 16: G[0x3178] <= value - 16 (SETENP@0x23a..0x248). |
| `0x125460` | encounter_entry_formations | high | investigation | ENINIT: the entry's two formations (ESINIT ids), into G[0x9529b] and G[0x9529c] (SETENP@0x2eb..0x306); the second's enemies stand five rows further (SETENP@0x6b7). Columns: 0=first, 1=second. |
| `0x125529` | formation_slot_units | high | investigation | ESINIT: the slot's enemy, an EBINIT unit (into G[0x9540c], SETENP@0x38a); 0 for an empty slot. SETENP@0x351 runs the base ESINIT only while slot 0's is 0. |
| `0x125531` | formation_slot_first_columns | high | investigation | ESINIT: the first column the enemy may stand in, from 1; 0 for any (SETENP@0x3d6..0x441). |
| `0x125539` | formation_slot_last_columns | high | investigation | ESINIT: the last column; when not above the first, the first only (SETENP@0x3d6..0x441). |
| `0x125541` | formation_slot_first_rows | high | investigation | ESINIT: the first row the enemy may stand in, from 1; 0 for any (SETENP@0x446..0x4b1). |
| `0x125549` | formation_slot_last_rows | high | investigation | ESINIT: the last row (SETENP@0x446..0x4b1). |
| `0x125551` | formation_slot_chances | high | investigation | ESINIT: the percent chance the slot is used; 0 for always; a draw at or above it drops the slot (SETENP@0x396..0x3be). |
| `0x125559` | encounter_chances | high | investigation | EEINIT: the percent chance of a battle at a field roll, by distance band (row: G[0x317a] / 20 below 10, 20, 40, 70, or more) and encounter level 0 to 9 (column: LAINIT's G[0xd9fae] or RMINIT's G[0xc5285], plus G[0x3181]) (FIELD@0x14b1..0x1589). |
| `0x12558d` | item_sort_keys | high | investigation | ITINIT: the list order key (10..29404); sort-indices-by-key-sum at CHMODE1@0x17bb, INFO4@0x8b7 and SHOP@0x1e26. |
| `0x12a3ad` | item_ranks | high | investigation | ITINIT: the rank 1..7 needed to use it. CHMODE1@0x1a5c..0x1ab1 refuses it (bit 0x11) when the character's rank at the item's class, G[0x8bace + 9*char + k], is lower; CHECKACT@0x8d, CHMODE3@0x18f. The "Cランク" descriptions hold 3. |
| `0x12cabd` | item_categories | high | investigation | ITINIT: the category, 1..46 (21..34 the magic schools, 41..46 the art schools; 8 also the enemies' actions and 9999 素手). ITMES@0x6 dispatches on it; CHMODE1@0x18c0 tests it against the character's mask G[0xa2ae5 + kind]; SHOP@0x27b0 draws its icon; CHECKACT@0x130 and CHMODE3@0x15f7 test the weapon an art needs. Values: 1 重要, 2 消費, 3 非戦闘, 4 杖, 5 投石, 6 短剣, 7 剣, 8 拳, 9 双剣, 10 連接, 11 槍, 12 弓, 13 衣服, 14 鎧, 15 盾, 16 装飾. |
| `0x12f1cd` | item_classes | high | investigation | ITINIT: the class, an AFINIT class: 1 重要, 2 通常, 3 非戦闘時, 4..6 weapon sizes, 7 軽装, 8 中装, 9 盾, 10 装飾, 21..35 magic, 40 enemy magic, 41..46 arts, 50 enemy arts. ADDITEM@0x78 treats class 1 as a key item; CHMODE1@0x1896 lists classes 1..20; CALCDAMAGE@0x672 uses the charge level above 20. Names: `item_class_names`, element 0 naming 0. |
| `0x1318dd` | item_damage_kinds | high | investigation | ITINIT: how the action deals damage. CALCDAMAGE@0x15d (3 and 4 take their own path), @0x352 (5, 戦意打撃), @0x49d (2), @0x731 (1); BTL@0x1788 charges kinds 2, 3 and 5 by 精速, the others by 肉速; SETCH@0x3e0 skips magic attack for 5. Values: 1 physical, 2 magic, 3 mind_check, 4 special, 5 fs_attack. |
| `0x133fed` | item_attack_elements | high | investigation | ITINIT: the attack element, 1..13. CALCDAMAGE@0x1b7 takes the action's, else the attacker's weapon's G[0x3896e3]; SETCH@0x548 sets the unit's from its weapon, else 1 物理 (SETCH@0x557). Names: `element_attack_names`, element 0 naming 0. |
| `0x1366fd` | item_defense_elements | high | investigation | ITINIT: the defense element an armour gives, 1..14. SETCH@0x56a takes it, else G[0xa175d + kind] (SETCH@0x595), into the unit's G[0x389701]; SETEN@0x7f9 alike; CALCDAMAGE@0x1e0 reads the unit's. Names: `element_defense_names`, element 0 naming 0. |
| `0x138e0d` | item_exclusive_users | high | investigation | ITINIT: the characters the item is exclusive to (145 items, up to 13 each). CHMODE1@0x19ea..0x1a45 refuses it (bit 0x10) unless G[0x8b158] is listed; SHOP@0x3a2a. 253 天王石【セリカ専用】 lists only 1. |
| `0x22d04d` | item_required_items | high | investigation | ITINIT: the item that must be held (count above 0) to use the action, in column 0: the summons' 喚石 (6..12) and the 招聘 stones (21..27), on actions 3121..3137. CHECKACT@0xda, CHMODE3@0x1595. |
| `0x23939d` | item_cursed_flags | high | investigation | ITINIT: 1 on the 8 cursed items, which cannot be taken off (CHMODE1@0x1c23); ADDILL@0x1bf..0x2c0 and CALCILL@0x69..0x139 unequip them when 呪い is cured. SHOP, SETCHSEL, DRAWCHP read it. |
| `0x23baad` | item_target_kinds | high | investigation | ITINIT: whom the action targets. CALCDAMAGE@0x19 (any but 1 takes the recovery path); CALCILL@0x2ef (1 goes through the boss check); USESKILL@0x10d (4). Self is inferred from the 自分の… descriptions. Values: 1 enemies, 2 allies, 3 self, 4 summon. |
| `0x23e1bd` | item_area_widths | high | investigation | ITINIT: the width of the area hit, 1..5; all 24 "範囲W×H" descriptions agree. |
| `0x2408cd` | item_area_heights | high | investigation | ITINIT: the height of the area hit, 1..5; all 24 "範囲W×H" descriptions agree. |
| `0x242fdd` | item_charge_times | high | investigation | ITINIT: the charge time before the action, less the unit's speed (BTL@0x177c); CHMODE3@0x1160 shows it. |
| `0x2456ed` | item_min_charge_times | high | investigation | ITINIT: the least charge time speed can bring it to (BTL@0x17f0). |
| `0x247dfd` | item_recovery_times | high | investigation | ITINIT: the recovery time after the action (BTL@0x2da9); CHMODE3 shows it. |
| `0x24a50d` | item_min_recovery_times | high | investigation | ITINIT: the least recovery time (BTL@0x2e1d). |
| `0x24cc1d` | item_condition_levels | high | investigation | ITINIT: condition levels by ILINIT condition id, positive inflicts and negative cures (129 女神の滴・大 −5 on 8..12, 14, 17, 19). CALCILL@0x51 loops 40 conditions, the level its absolute value (CALCILL@0x256); BTL, CHECKACT, the RTN scripts, USESKILLSUB read it. Column k is record k of `condition`. |
| `0x2ae69d` | item_equip_condition_levels | high | investigation | ITINIT: the lasting condition levels an equipped item gives, by ILINIT id. SETCH@0x5fc..0x61d adds them into the unit's G[0x389cbf + 40*unit + cond]; SETEN, CALCREVISE. 2927 メチュカの微笑み "命中+30 痛打+10" holds {20: 5, 34: 5}. Column k is record k of `condition`. |
| `0x31011d` | item_accuracy_bonuses | high | investigation | ITINIT: 命中; SETCH adds it into the unit's G[0x38971f] (CALCDAMAGE@0x290). 77 of 80 descriptions agree. |
| `0x31282d` | item_evasion_bonuses | high | investigation | ITINIT: 回避, into the unit's G[0x38973d] (SETCH). 34 of 37 descriptions agree. |
| `0x314f3d` | item_physical_attack_bonuses | high | investigation | ITINIT: physical attack ("攻撃+"), into the unit's G[0x3894e5] (SETCH; CALCDAMAGE@0x5e9). 71 of 71 descriptions agree. |
| `0x31764d` | item_physical_defense_bonuses | high | investigation | ITINIT: physical defense, into the unit's G[0x389503] (SETCH). 54 of 54 descriptions agree. |
| `0x319d5d` | item_attack_count_bonuses | high | investigation | ITINIT: attack count, into the unit's G[0x38955d] (SETCH; CALCDAMAGE@0x560). 69 of 72 descriptions agree. |
| `0x31c46d` | item_defense_count_bonuses | high | investigation | ITINIT: defense count, into the unit's G[0x38957b] (SETCH). 44 of 50 descriptions agree. |
| `0x31eb7d` | item_magic_attack_bonuses | high | investigation | ITINIT: magic attack, into the unit's G[0x389521] (SETCH; CALCDAMAGE@0x520). For a damage kind 5 action it is the 戦意 attack instead: SETCH@0x3e0 skips it and CALCDAMAGE@0x367 uses it. 71 of 73 and 11 of 11 descriptions agree. |
| `0x32128d` | item_magic_defense_bonuses | high | investigation | ITINIT: magic defense, into the unit's G[0x38953f] (SETCH). 67 of 67 descriptions agree. |
| `0x32399d` | item_magic_power_bonuses | high | investigation | ITINIT: magic power, into the unit's G[0x389779] (SETCH; CALCDAMAGE@0x4f6). 50 of 51 descriptions agree. |
| `0x3260ad` | item_physical_resistance_bonuses | high | investigation | ITINIT: physical resistance (肉抵), into the unit's G[0x389599] (SETCH); an action's value adds to the physical roll (CALCILL@0x37a). SETILLREVISE's 肉抵 writes the same unit cell. |
| `0x3287bd` | item_mental_resistance_bonuses | high | investigation | ITINIT: mental resistance (精抵), into the unit's G[0x3895b7] (SETCH; CALCILL@0x338, CALCDAMAGE@0x373). |
| `0x32aecd` | item_physical_speed_bonuses | high | investigation | ITINIT: physical battle speed (肉速), into the unit's G[0x3895d5] (SETCH), which BTL@0x1788 subtracts from the charge time. |
| `0x32d5dd` | item_mental_speed_bonuses | high | investigation | ITINIT: mental battle speed (精速), into the unit's G[0x3895f3] (SETCH), used for damage kinds 2, 3 and 5 (BTL@0x1788). |
| `0x32fced` | item_luck_bonuses | high | investigation | ITINIT: luck (運), into the unit's G[0x389611] (SETCH); SETILLREVISE's 運 writes the same cell. |
| `0x3323fd` | item_critical_bonuses | high | investigation | ITINIT: critical (痛打), into the unit's G[0x38975b] (SETCH); CALCDAMAGE@0x3f1..0x412 rolls the critical hit with it. |
| `0x334b0d` | item_hp_amounts | high | investigation | ITINIT: HP restored (positive) or spent (negative). CALCDAMAGE@0x33..0x8a adds item_random_amounts and caps at 99999; USESKILLSUB@0x23. 101 治癒の水・小 holds 497 for "HP+500". |
| `0x33721d` | item_mp_amounts | high | investigation | ITINIT: MP restored, or spent when negative (the spells' costs; 90 of 90 "/MPn】" agree). CALCDAMAGE@0x8f; USESKILLSUB@0xb2. |
| `0x33992d` | item_tp_amounts | high | investigation | ITINIT: TP restored, or spent when negative (43 of 43 "TPn】" agree). CALCDAMAGE@0xeb; USESKILLSUB@0x141. |
| `0x33c03d` | item_fs_amounts | high | investigation | ITINIT: 戦意 restored (6 of 6 descriptions agree). CALCDAMAGE@0x147 sets G[0x952aa] to its negative; USESKILLSUB@0x1d0. |
| `0x33e74d` | item_control_rate_changes | high | investigation | ITINIT: 迷宮支配力 gained, 5, 10 or 15 (3 of 3 descriptions agree); USESKILL@0xd9..0x108 adds it to the saved G[0x3178], at most 15. SETCHSEL reads it. |
| `0x34356d` | item_max_hp_bonuses | high | investigation | ITINIT: max HP, into the unit's G[0x38944f] (SETCH@0x30e; 7 of 7 descriptions agree). Negative on summons: CALCREVISE@0x0..0x91 adds action 3121 + b's for each bit b of G[0x8e3d2 + char]. |
| `0x345c7d` | item_max_mp_bonuses | high | investigation | ITINIT: max MP, into the unit's G[0x38948b] (SETCH@0x338). The summons' −30..−60 are their "MP&TP n低下" (3121 パズモ召喚 −30); CALCREVISE. |
| `0x34838d` | item_max_tp_bonuses | high | investigation | ITINIT: max TP, into the unit's G[0x3894c7] where the unit has TP (SETCH@0x362); the summons' negatives as for MP; CALCREVISE. |
| `0x34d1ad` | item_random_amounts | high | investigation | ITINIT: the random part added to an amount, 5..100 (CALCDAMAGE@0x5b, @0xb7, @0x113, @0x5fc; USESKILLSUB). |
| `0x34f8bd` | item_animations | high | investigation | ITINIT: the battle animation; BTL@0x1ca9 and @0x2127 put it in G[0x38a9f8] and call ANIME. |
| `0x351fcd` | item_buy_prices | high | investigation | ITINIT: the shop's price (267 items); SHOP@0x10db, @0x11ff, @0x28e7. |
| `0x3546dd` | item_sell_prices | high | investigation | ITINIT: what the shop pays, a quarter of the buy price on 259 of 267; SHOP@0x1143, @0x1252, @0x1642. |
| `0x356ded` | item_menu_use_masks | high | investigation | ITINIT: where the item can be used from the menu. CHMODE1@0x19c9 ANDs it with 1 while G[0x317c] is set (in the dungeon, FIELD@0x99) and with 2 otherwise (CHMODE1@0x206..0x215); in battle CHMODE1@0x1997 tests class 2 instead. CHMODE3 reads it. Values: 1 in_dungeon, 3 anywhere. |
| `0x3594ff` | condition_resistances | high | investigation | ILINIT: which resistance each level is rolled against (CALCILL@0x262..0x28d, @0x31f..0x39b). Values: 3 mental, 4 physical. |
| `0x3595c7` | condition_affinity_columns | high | investigation | ILINIT: the column of element_affinities, 11..17 (抵抗1..7), that scales the condition's chance (CALCILL@0x3b3, the matrix at CALCILL@0x3c3). |
| `0x35968f` | condition_cure_bits | med | investigation | ILINIT: which recoveries clear the condition, 2 or 3. RECOVER@0x8f sets the level to its floor when the bits meet 1 (while G[0x317c] is set, in the dungeon) or 2 (outside) (RECOVER@0x3a..0x49); BTL@0x367a resets a unit's level to its lasting level for bit 2. |
| `0x359757` | condition_durations | high | investigation | ILINIT: how long each level lasts, −1 for 呪い (until cured). ADDILL@0x16d..0x1ba copies it into the character's G[0x8d3ce + 40*char + cond]; CALCILL, DRAWENLIST read it. |
| `0x35981f` | condition_hit_modifiers | high | investigation | ILINIT: 命中 added (SETILLREVISE@0x13e into the unit's G[0x38971f]); 呪い, 酔い, 命中. |
| `0x3598e7` | condition_evasion_modifiers | high | investigation | ILINIT: 回避 added (SETILLREVISE@0x155 into G[0x38973d]); 呪い, 麻痺, 酔い, 回避. |
| `0x3599af` | condition_physical_attack_modifiers | high | investigation | ILINIT: physical attack, in percent (SETILLREVISE@0x41, scaled at SETILLREVISE@0x1e2..0x267, into G[0x3894e5]); 高揚, 物攻. |
| `0x359a77` | condition_physical_defense_modifiers | high | investigation | ILINIT: physical defense, in percent (SETILLREVISE@0x58); 高揚, 物防. |
| `0x359b3f` | condition_attack_count_modifiers | high | investigation | ILINIT: attack count added (SETILLREVISE@0x9d); 攻回. |
| `0x359c07` | condition_defense_count_modifiers | high | investigation | ILINIT: defense count added (SETILLREVISE@0xb4); 防回. |
| `0x359ccf` | condition_magic_attack_modifiers | high | investigation | ILINIT: magic attack, in percent (SETILLREVISE@0x6f); 高揚, 魔攻. |
| `0x359d97` | condition_magic_defense_modifiers | high | investigation | ILINIT: magic defense, in percent (SETILLREVISE@0x86); 高揚, 魔防. |
| `0x359e5f` | condition_magic_power_modifiers | high | investigation | ILINIT: magic power added (SETILLREVISE@0x183 into G[0x389779]); 魔威. |
| `0x359f27` | condition_physical_resistance_modifiers | high | investigation | ILINIT: physical resistance added (SETILLREVISE@0xcb into G[0x389599]); 肉抵. |
| `0x359fef` | condition_mental_resistance_modifiers | high | investigation | ILINIT: mental resistance added (SETILLREVISE@0xe2 into G[0x3895b7]); 精抵. |
| `0x35a0b7` | condition_physical_speed_modifiers | high | investigation | ILINIT: physical speed added (SETILLREVISE@0xf9 into G[0x3895d5]); 呪い, 束縛, 肉速. |
| `0x35a17f` | condition_mental_speed_modifiers | high | investigation | ILINIT: mental speed added (SETILLREVISE@0x110 into G[0x3895f3]); 呪い, 束縛, 精速. |
| `0x35a247` | condition_luck_modifiers | high | investigation | ILINIT: luck added (SETILLREVISE@0x127 into G[0x389611]); 運. |
| `0x35a30f` | condition_critical_modifiers | high | investigation | ILINIT: critical added (SETILLREVISE@0x16c into G[0x38975b]); 呪い, 高揚, 痛打. |
| `0x35a3d7` | condition_hp_changes | high | investigation | ILINIT: HP change each frame in battle and each step in the dungeon (猛毒 −1..−5, 再生 +): SETILLREVISE@0x19a into the unit's G[0x389797]; FIELD@0x11f1 adds it to the character's HP G[0x8b42a]. |
| `0x35a49f` | condition_mp_changes | med | investigation | ILINIT: MP change each frame and step, read (SETILLREVISE@0x1b1, FIELD@0x1211) but written by no condition. |
| `0x35a567` | condition_tp_changes | med | investigation | ILINIT: TP change each frame and step, read (SETILLREVISE@0x1c8, FIELD@0x1231) but written by no condition. |
| `0x35a62f` | condition_fs_changes | high | investigation | ILINIT: 戦意 change each 10 frames in battle (強酸, 恐怖: −5..−10): BTL@0x5b7..0x625 adds it to the unit's G[0x3893f5] when its timer mod 10 is 1, at least 1. 恐怖 "戦闘中は10フレーム毎に入力値がそのまま引かれる". |
| `0x35a6f7` | condition_listed_flags | med | investigation | ILINIT: 1 on 石化 and 7..19, written before each name: DRAWENLIST@0x71 lists each such condition a unit has as its own entry; any other condition above its lasting level adds one shared entry, 0x1d (DRAWENLIST@0xb5..0xf4). |
| `0x35a787` | routine_step_kinds | high | investigation | RTINIT: each step's rule, 1 to 12, 101 to 103, 201 and 301 to 306 for RTN001 to RTN306 (RTN@0x0..0x6e maps rule r to the script in local 5 + r); 0, or a rule with no script, ends the routine (RTN@0xef..0x122). |
| `0x35f5a7` | routine_step_chances | high | investigation | RTINIT: the percent chance the step runs (RTN@0x1a1 draws 0 to 99 and goes on when the draw is below it). |
| `0x3643c7` | routine_step_actions | high | investigation | RTINIT: the step's action: below 6 a slot of the unit's six actions G[0x3866a7], otherwise an ITINIT id (RTN003@0x0..0x2d); an item for rules 301 to 306, which a party battler must hold (RTN301@0x25, RTN304@0x45). |
| `0x3691e7` | routine_step_args | high | investigation | RTINIT: the step's first argument: the gauge (1 HP, 2 MP, 3 TP, 4 FS (戦意), G[0x3893f5], whose start SETEN@0xa49 takes from the enemy slot) for rules 4 to 6, 10, 11, 101 to 103 and 304 to 306 (RTN304@0x66..0x148); the unit for rule 7; how many targets must be in reach for 9 and 302; the battle time 100 * G[0x9529f] + G[0x9529e] for 12 and 303 (RTN012@0x20). |
| `0x36e007` | routine_step_percents | high | investigation | RTINIT: the percent of the gauge's maximum the gauge test compares with, rules 5, 6, 10, 11, 101 to 103 and 304 to 306 (RTN304@0x20, @0x14f). |
| `0x372e27` | routine_step_target_counts | high | investigation | RTINIT: how many battlers must pass the gauge test, rules 103 and 306 (RTN103@0x30, @0x215). |
| `0x37ca67` | routine_step_flags | med | investigation | RTINIT: when set, the step runs only while G[0x470 + value - 1000] is set (RTN@0x12f..0x15a); no routine sets one. |
| `0x381887` | routine_step_limits | high | investigation | RTINIT: how many times the step may run in a battle, against routine_step_run_counts (RTN@0x15f..0x195). |
| `0x3866ad` | element_affinities | high | investigation | AFINIT: the damage percentage by defense element (row 0..14) and attack element (column 0..17). Row 0 and column 0 are 100, column 10 is 0; the game names column 0 none (OME's word: undefined), and SETCH gives a party unit with no weapon element 1, so a party attack never reads it; 火炎 takes −100 of 火炎 and 150 of 冷却. CALCDAMAGE@0x909 and @0x3ac; a negative value gives negative damage (CALCDAMAGE@0x930..0x953). CALCILL@0x3c3 reads columns 11..17 for conditions. Columns: 0=undefined. Column names: `element_attack_names`, element 0 naming column 0. 0 is a value, not none. |
| `0x38683d` | item_class_icons | med | investigation | AFINIT: the class's icon, a cell of the 10-wide sheet of 12x14 icons the categories also use (CHMODE2@0x23c2, RESULT@0xf4c). |
| `0x386879` | item_class_required_weapon_masks | high | investigation | AFINIT: the weapon categories an art school needs, bit k for item_categories k: 41 = 704 (短剣, 剣, 双剣), 42 = 3968, 43 = 4096 (弓). CHECKACT@0x114, CHMODE3@0x15db; the notes agree. |
| `0x3868b5` | item_class_rank_exps | med | investigation | AFINIT: the most class exp a rank holds, by rank − 1 (weapons 10000 … 150000): BTL@0x2c32..0x2c8d caps the character's G[0x8be52 + 9*char + k] at it; CHMODE2@0x1569, CHMODE3@0xf9f. |
| `0x386a1d` | item_category_weapon_flags | med | investigation | AFINIT, by item_categories 4..12: bit 1 (杖, 短剣, 剣, 拳, 双剣) halves damage past a unit in between (CALCDAMAGE@0x776); bit 0 (双剣, 連接, 槍, 弓) clears or blocks the slot G[0x8c302] (CHMODE1@0x12b9, @0x1409, @0x15ee; inferred two-handed). |
| `0x386a59` | world_map_pictures | high | investigation | INIT2@0x74..0x83: the world map's picture for each story part, [1] to [4] AL001A.AGF to AL004A.AGF ([0] is not written). ALLMAP@0x967, SHOWALLMAP@0x8f and TOWN@0x5cb set [story_part] as texture 0x3b. |
| `0x389318` | character_sort_keys | low | investigation | INIT2's inline arrays: [1..9] = 1..9 (INIT2@0x97), [51..57] = 10, 11, 12, 13, 14, 16, 15 (INIT2@0x9c). CHMODE1@0x21a and CHMODE2@0x1c0 sort characters by it (sort-indices-by-key-sum). |
| `0x38a9fa` | battle_animation_effects | high | investigation | BTANINIT2: each of the six slots' effect, a BTANINIT id; slot 5 plays only on a hit, G[0x952a4] (ANIME@0x4c1..0x4fb), and slots 0 and 1 only while bit 0 of G[0x38a9f9] is set (ANIME@0x243). |
| `0x39945a` | battle_animation_effect_starts | high | investigation | BTANINIT2: when each slot's effect starts, in ms (ANIME@0xbca, @0xc2d). |
| `0x3a7eba` | battle_animation_lengths | high | investigation | BTANINIT2: the animation's length in ms (ANIME@0x2b2); a voice is scheduled at it (ANIME@0x1022) and an end fade runs at it plus 300 to 400 (ANIME@0x12ab..0x1330). |
| `0x3aa5ca` | battle_animation_split_styles | med | investigation | BTANINIT2: how the battle picture splits as it slides away after the animation, 250 ms (BTL@0x6352..0x6701): 1 and 2 two-pixel strips, 3 left and right halves, 4 top and bottom halves, 5 three bands, 6 eight bands; skipped while G[0x8f566] or G[0x90510] is set (BTL@0x6328). |
| `0x3accda` | battle_effect_pictures | high | investigation | BTANINIT: the slot's picture, an MVB...A.AGF movie (106 effects) or an AM... sheet (27); 0 for an empty slot (ANIME@0xa6c; the movie at @0xc3f, the texture at @0xb9d). |
| `0x3acce0` | battle_effect_still_kinds | high | investigation | BTANINIT: 0 for a movie; 1 a texture with colour key 0xff00, 2 a texture with key -1 (ANIME@0xb5e..0xbbc); textures are set only while bit 0 of G[0x38a9f9] is set. |
| `0x3acce6` | battle_effect_blend_modes | med | investigation | BTANINIT: when set, ANIME draws the effect with gfx-draw-color mode 1, 0xff, 0xffffff (ANIME@0xc5b..0xc70); 115 effects have 1. |
| `0x3accec` | battle_effect_widths | high | investigation | BTANINIT: the effect's width; it is drawn centred on its point (ANIME@0xae1, @0xc16). |
| `0x3accf2` | battle_effect_heights | high | investigation | BTANINIT: the effect's height (ANIME@0xaf6). |
| `0x3accf8` | battle_effect_sheet_columns | high | investigation | BTANINIT: a sheet's columns, for animate-gfx-srcrect-target (ANIME@0xbf1); 27 effects. |
| `0x3accfe` | battle_effect_sheet_rows | low | investigation | BTANINIT: a sheet's rows, inferred: ANIME does not read it, but columns * rows is the frame count in all 27 sheets. |
| `0x3acd04` | battle_effect_frame_counts | high | investigation | BTANINIT: a sheet's frame count (ANIME@0xbe3). |
| `0x3acd0a` | battle_effect_lengths | high | investigation | BTANINIT: the effect's length in ms; for a movie ANIME writes the movie's own length into it (ANIME@0xc56) and keeps the latest end (ANIME@0xc85..0xccc). |
| `0x3acd10` | battle_effect_placed_flags | high | investigation | BTANINIT: 1 to place the effect at the target's side (x 0x76 or 0x2aa, y 0xdc), 0 at the screen's centre, 400 by 300 (ANIME@0xa7a..0xadc). |
| `0x3acd16` | battle_effect_x_offsets | high | investigation | BTANINIT: added to the effect's x (ANIME@0xb0b); 0 in every effect. |
| `0x3acd1c` | battle_effect_y_offsets | high | investigation | BTANINIT: added to the effect's y (ANIME@0xb1e); 0 in every effect. |
| `0x3acd22` | battle_effect_sounds | high | investigation | BTANINIT: the effect's sound (A0120.WAV ...), played on channel 3 + slot (ANIME@0x1055..0x1071). |
| `0x3acd28` | battle_effect_sound_delays | high | investigation | BTANINIT: the sound's delay in ms after the effect starts (ANIME@0x1088..0x109d). |
| `0x3acd2e` | battle_effect_cue_times | med | investigation | BTANINIT: up to three moments, in ms, at which the picture object 0x4651 shakes by 4 pixels for 100 ms, only on a hit G[0x952a4] (ANIME@0x11b7..0x1287, the shake at @0x1129..0x114c); 8 effects. |
| `0x3acd41` | profile_unit_ids | high | investigation | CIINIT: the profile's unit; INFO2@0x00844 lists profiles with one, and INFO2@0x00876 shows the name only once the unit's heard flag G[0x8fd3d + unit] is set; the table's leader. |
| `0x3acda5` | profile_pictures | high | investigation | CIINIT: the profile's picture (INFO2@0x00e0f). |
| `0x3ace09` | profile_picture_x | high | investigation | CIINIT: the picture's x offset (INFO2@0x00e40). |
| `0x3ace6d` | profile_picture_y | high | investigation | CIINIT: the picture's y offset (INFO2@0x00e53). |
| `0x3aced2` | glossary_unlock_scenes | high | investigation | VIINIT: up to three scenes, each + 1, that unlock entry [id] (column 0 on all 90 entries, 1 on 7, 2 on 2). INFO1@0x8ba..0x921 lists the entry when any of them has scene_seen_flags set. One value, 995, names scene 994, which SCINIT does not define. |
| `0x3ad12a` | glossary_pictures | high | investigation | VIINIT: the entry's picture (CI502A.AGF ...). INFO1@0xeaa sets it as a texture and draws it at (590 - width / 2, 600 - height) plus the offsets. |
| `0x3ad1f2` | glossary_picture_x_offsets | high | investigation | VIINIT: the picture's x offset, -23 on all 90 entries (INFO1@0xedb). |
| `0x3ad2ba` | glossary_picture_y_offsets | high | investigation | VIINIT: the picture's y offset, -203 on all 90 entries (INFO1@0xeee). |
| `GS 0x6e` | — | low | auto-shape | array |
| `GS 0x4c9` | — | low | auto-shape | array |
| `GS 0x4dd` | — | low | auto-shape | array |
| `GS 0x5a6` | — | low | auto-shape | array |
| `0x5b0` | — | low | auto-shape | array |
| `0x2fd3` | — | low | auto-shape | array |
| `0x302a` | — | low | auto-shape | array |
| `0x303e` | — | low | auto-shape | array |
| `0x3052` | — | low | auto-shape | array |
| `0x3066` | — | low | auto-shape | array |
| `0x307a` | — | low | auto-shape | array |
| `0x308e` | — | low | auto-shape | array |
| `GS 0x812f` | — | low | auto-shape | array |
| `0x8b14c` | — | low | auto-shape | array |
| `0x8b151` | — | low | auto-shape | array |
| `0x8b156` | — | low | auto-shape | array |
| `0x8b22f` | — | low | auto-shape | array |
| `0x8b29a` | — | low | auto-shape | array |
| `0x8b3c6` | — | low | auto-shape | array |
| `0x8c23a` | — | low | auto-shape | array |
| `0x8c29e` | — | low | auto-shape | array |
| `0x8e3d2` | — | low | auto-shape | array |
| `0x8e436` | — | low | auto-shape | array |
| `0x8e49a` | — | low | auto-shape | array |
| `0x8e4fe` | — | low | auto-shape | array |
| `0x8e562` | — | low | auto-shape | array |
| `0x90125` | — | low | auto-shape | array |
| `0x94f68` | — | low | auto-shape | array |
| `0x94f7d` | — | low | auto-shape | array |
| `0x95045` | — | low | auto-shape | array |
| `0x95113` | — | low | auto-shape | array |
| `0x9511e` | — | low | auto-shape | array |
| `0x95182` | — | low | auto-shape | array |
| `0x951ea` | — | low | auto-shape | array |
| `0x95250` | — | low | auto-shape | array |
| `0x9526e` | — | low | auto-shape | array |
| `0x9528c` | — | low | auto-shape | array |
| `0x9529b` | — | low | auto-shape | array |
| `0x952ac` | — | low | auto-shape | array |
| `0x95311` | — | low | auto-shape | array |
| `0xc56a4` | — | low | auto-shape | array |
| `0xc56cc` | — | low | auto-shape | array |
| `0xc5730` | — | low | auto-shape | array |
| `0xc57c5` | — | low | auto-shape | array |
| `0xc57ed` | — | low | auto-shape | array |
| `0xc5815` | — | low | auto-shape | array |
| `0xc583d` | — | low | auto-shape | array |
| `0xc5865` | — | low | auto-shape | array |
| `0xc588d` | — | low | auto-shape | array |
| `0xc58b5` | — | low | auto-shape | array |
| `0xc58dd` | — | low | auto-shape | array |
| `0xc5905` | — | low | auto-shape | array |
| `0xc592d` | — | low | auto-shape | array |
| `0xc5955` | — | low | auto-shape | array |
| `0xc597d` | — | low | auto-shape | array |
| `0xcf685` | — | low | auto-shape | array |
| `0xcf74d` | — | low | auto-shape | array |
| `0xd46fd` | — | low | auto-shape | array |
| `0xd4761` | — | low | auto-shape | array |
| `0xd47c5` | — | low | auto-shape | array |
| `0xd4829` | — | low | auto-shape | array |
| `0xd488d` | — | low | auto-shape | array |
| `0xd48f1` | — | low | auto-shape | array |
| `0xd4da1` | — | low | auto-shape | array |
| `0x34aa9d` | — | low | auto-shape | array |
| `0x3866a7` | — | low | auto-shape | array |
| `0x386a70` | — | low | auto-shape | array |
| `0x389180` | — | low | auto-shape | array |
| `0x389188` | — | low | auto-shape | array |
| `0x3891ec` | — | low | auto-shape | array |
| `0x389250` | — | low | auto-shape | array |
| `0x3892b4` | — | low | auto-shape | array |
| `0x38937d` | — | low | auto-shape | array |
| `0x38939b` | — | low | auto-shape | array |
| `0x3893b9` | — | low | auto-shape | array |
| `0x3893d7` | — | low | auto-shape | array |
| `0x3893f5` | — | low | auto-shape | array |
| `0x389413` | — | low | auto-shape | array |
| `0x389431` | — | low | auto-shape | array |
| `0x38944f` | — | low | auto-shape | array |
| `0x38946d` | — | low | auto-shape | array |
| `0x38948b` | — | low | auto-shape | array |
| `0x3894a9` | — | low | auto-shape | array |
| `0x3894c7` | — | low | auto-shape | array |
| `0x3894e5` | — | low | auto-shape | array |
| `0x389503` | — | low | auto-shape | array |
| `0x389521` | — | low | auto-shape | array |
| `0x38953f` | — | low | auto-shape | array |
| `0x38955d` | — | low | auto-shape | array |
| `0x38957b` | — | low | auto-shape | array |
| `0x389599` | — | low | auto-shape | array |
| `0x3895b7` | — | low | auto-shape | array |
| `0x3895d5` | — | low | auto-shape | array |
| `0x3895f3` | — | low | auto-shape | array |
| `0x389611` | — | low | auto-shape | array |
| `0x38962f` | — | low | auto-shape | array |
| `0x38964d` | — | low | auto-shape | array |
| `0x38966b` | — | low | auto-shape | array |
| `0x389689` | — | low | auto-shape | array |
| `0x3896a7` | — | low | auto-shape | array |
| `0x3896c5` | — | low | auto-shape | array |
| `0x3896e3` | — | low | auto-shape | array |
| `0x389701` | — | low | auto-shape | array |
| `0x38971f` | — | low | auto-shape | array |
| `0x38973d` | — | low | auto-shape | array |
| `0x38975b` | — | low | auto-shape | array |
| `0x389779` | — | low | auto-shape | array |
| `0x389797` | — | low | auto-shape | array |
| `0x3897b5` | — | low | auto-shape | array |
| `0x3897d3` | — | low | auto-shape | array |
| `0x38a61f` | — | low | auto-shape | array |
| `0x38a63d` | — | low | auto-shape | array |
| `0x38a65b` | — | low | auto-shape | array |
| `0x38a679` | — | low | auto-shape | array |
| `0x38a697` | — | low | auto-shape | array |
| `0x38a6b5` | — | low | auto-shape | array |
| `0x38a6d3` | — | low | auto-shape | array |
| `0x38a6f1` | — | low | auto-shape | array |
| `0x38a70f` | — | low | auto-shape | array |
| `0x38a72d` | — | low | auto-shape | array |
| `0x38a74b` | — | low | auto-shape | array |
| `0x38a769` | — | low | auto-shape | array |
| `0x38a9df` | — | low | auto-shape | array |

## index-pointer

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x0` | system_mode | high | investigation | Saved. The mode SYSTEM4 runs each pass (SYSTEM4@0x2ea..0x5d6): 1 a scene, 2 ALLMAP, 3 FIELD, 4 BTL, 5 TOWN; any other value shows 不正な実行モード and asks to exit (SYSTEM4@0x5dc). A SCJUMP rule that plays a scene sets 1 (SCJUMP@0x65); a scene's end sets 3, 2 or 5 (SC0000@0x4810, SC0000@0xf986..0xf9c3). Values: 1 scene, 2 ALLMAP, 3 FIELD, 4 BTL, 5 TOWN. |
| `0x454` | current_scene_script_id | high | investigation | Saved. SYSTEM4@0x44a..0x474 copies scene_script_ids[current_scene_id] into it when scene_run_mode is 0 or -2 (0 becomes 0x28, SC0000.BIN, @0x479), then runs it (@0x48c). TITLE's debug menu sets it to the chosen script (TITLE@0x685, 0x6aa, 0x6c8). |
| `0x2fd0` | adv_speaker_id | high | investigation | The page's speaker: an index into CNINIT's names (speaker_names), or -1 for narration. Every spoken page sets it, draws the name plate from it (SC0470@0x00b81) and mutes the voice when it is below 0 or its voice setting's switch is set (SC0470@0x00bb8..0x00bdf); a page without a speaker sets -1 (`$1$MPEV49F0101@0x00194`). |
| `0x2fd2` | scjump_context_id | high | investigation | Saved. Which of SCJUMP's rule lists runs (SCJUMP@0x0). 1 ALLMAP@0x2c each world map pass; 2 FIELD@0x136 and TOWN@0x34 on entry (rules test story_part and the map G[0x30b2]); 3 TOWN@0x51d for each location (rules test town_site_id); 5 BTL@0x7065 after a battle; 6 MENU@0x585, the escape command. 4 is never set and its list is empty. BTL and MENU restore the old value after. |
| `0x30b0` | story_part | high | investigation | Saved: the story's part, 1 to 4, and with it the world map in play. GAMESTART@0x665 (通常) and TITLE@0x633 set 1, SC0180@0x11ae0 2, SC0500@0x129a8 3 and SC0390@0x1b880 4, as GAMESTART@0x423 does for 特別編. AMINIT dispatches on it (AMINIT@0x32), world_map_pictures is indexed by it, SCJUMP's rules test it, and CHANGECHAPTER@0x1b..0x40 turns it into lead_unit_id. Not the chapter: the chapter's name is GS[0xdc], which each scene sets (SC0000@0x7be). |
| `0x30b1` | lead_unit_id | high | investigation | Saved: セリカ's unit for the world map in play, 1 to 4. CHANGECHAPTER@0x1b..0x3d sets it from story_part, @0x40 makes it his unit (character_unit_ids[1]), and @0x45, @0x18a and @0x37f read CBINIT's row and that unit's EBINIT fields by it. CPINIT's field sheets are read by it too (FIELD@0x1806, DRAWJUMP@0xe8). |
| `0x30b2` | current_place_id | high | investigation | Saved: the place in play, an area id: a map 1 to 49 (FIELD) or a town from 50 (TOWN). ALLMAP@0x78d sets it on entering an area; MPINIT, RMINIT and SIINIT are read by it (FIELD@0xba, FIELD@0x88, TOWN@0x6a) and RMINIT and SIINIT branch on it. |
| `0x3149` | town_site_id | high | investigation | Saved: the town site TOWN is about to offer: TOWN@0x518 sets it for each site, and TOWN@0x51d runs SCJUMP's context 3, every rule of which tests it (SCJUMP@0x1480). |
| `0x3169` | map_block_row | high | investigation | Saved: the floor, the row of the block in play; RMINIT's arrays are read at 30 * row + column (FIELD@0x5c7, FIELD@0x606) and MPINIT's floor names at 100 * map + row (FIELD@0x170). MPINIT's start floor is written here (FIELD@0xba..0xf1). |
| `0x316a` | map_block_column | high | investigation | Saved: the section, the block in play within its row (FIELD@0x5c7, FIELD@0x606); MPINIT's start section is written here. |
| `0x316c` | map_section_slot | high | investigation | Saved: the slot of map_section_codes the section in play holds, found or taken at FIELD@0x1a7..0x230; the map object arrays are read by it (DRAWMAP@0x3c6). |
| `0x3174` | map_player_facing | high | investigation | Saved: the player's facing, set from the walking direction (DRAWWALK@0x0..0x130, screen y downward): 0 down-left, 1 left, 2 up-left, 3 up, 4 up-right, 5 right, 6 down-right, 7 down. |
| `0x3175` | map_player_target_facing | med | investigation | Saved: MPINIT's start facing is written here on entry, and FIELD@0x18e..0x198 copies it into map_player_facing when map_continue is 0. |
| `0x3176` | field_land_id | high | investigation | The land type under the player: FIELD@0x1734 writes AGERC _GetLand's (slot 2) answer here. LAINIT's arrays are read by it; 0 is no land entry. |
| `0x317f` | map_music_variant | high | investigation | Saved: which of the block's three musics plays (FIELD@0x606..0x64c); map scripts set it. |
| `0x3180` | map_encounter_variant | high | investigation | Saved: which of the block's two encounter sets is in play (FIELD@0x5c7..0x5e0); map scripts set it. |
| `0x8b158` | character_arg_id | high | investigation | The character the shared character scripts work on: ADDEXP, ADDSKILL, BTLMENU, CALCLVUP, CALCNEXTEXP, CALCREVISE, LEAVECH, RETIRECH, SETCH and USESKILL read it and never set it. A caller sets it before running them (83 scripts do), and INIT2@0xb9..0xeb loops it over characters 1 to 99 for CALCREVISE. It lies below the saved prefix, so a save holds its last value. |
| `0x8b225` | formation_current | high | investigation | Saved: the party's formation, 1..8; CHMODE4@0x1639 sets it, CHMODE4@0x1c42 draws formation_names[f]; GAMESTART@0x68c starts with 1. SETCH and SETFORM read it. |
| `0x91865` | script_arg_1 | high | investigation | The first argument a script passes to the one it calls, set before call-script. ADDHP, ADDMP and ADDTP (@0x1), GETMAPEVF (@0x5), GETMONEYMES (@0x378) and ADDEXP (@0x16) read their input from it; SHOWALLMAP takes it as the story part whose world map it shows (SHOWALLMAP@0xf), restoring the old part at @0x375; ADDEXP passes CALCLVUP a character's new level in it (ADDEXP@0x114, CALCLVUP@0x13). |
| `0x91866` | script_arg_2 | high | investigation | The second argument, set beside script_arg_1. GETMAPEVF reads it (@0xa); ADDEXP passes CALCLVUP the level before in it (ADDEXP@0x119), and CALCLVUP takes script_arg_1 - script_arg_2 (@0x18); SHOWALLMAP takes it as the area to highlight with its area_pictures column 1 (SHOWALLMAP@0x14, @0xb2), standing in current_place_id until @0x375. |
| `0x9186a` | played_scene_id | med | investigation | The scene a map event's inline scene pass last played. MPEV01B0102@0x21a copies current_scene_id into it and repeats the pass while the two differ (@0x20c), so a scene that sets current_scene_id chains to the next; 283 map events hold this loop. |
| `0xc1e5b` | shop_id | high | investigation | The shop SHOP opens, 1..12: a scene sets it before calling SHOP (SC1500@0x9be), SHINIT branches on it, and SHOP@0x3b1d clears it. |
| `0xc1e73` | arena_rank_in_play | high | investigation | The arena rank ARENA shows, 0 to 3 (ARENA@0x96b); the first index of ARINIT's round arrays (ARENA@0x691, SETEN@0x45, RESULT@0x307). |
| `0xc1e74` | arena_round_in_play | high | investigation | The round chosen in ARENA, 0 to 6; it starts at arena_rounds_won (ARENA@0xbb) and moves on after a first win (ARENA@0x7de). |
| `0x10cc92` | treasure_current_id | high | investigation | The treasure GETITEM gives: GETTREASURE sets it from the map object in play (GETTREASURE@0x0), GAMESTART@0x5a5 and @0x691 to the starting kits 9998 and 9999; GETITEM@0x15f clears it. |
| `0x125333` | encounter_set_in_play | high | investigation | The encounter set in play, ENINIT's selector, from RMINIT's G[0xc4b7d + 60 * row + 2 * block + G[0x3180]] (FIELD@0x5e0) or a map script (MP02B0207@0x276). |
| `0x125528` | formation_in_play | high | investigation | The formation ESINIT loads, G[0x9529b] or G[0x9529c] (SETENP@0x342). |
| `0x12558b` | current_item | high | investigation | The item ADDITEM, LOSTITEM and ITMES work on: a caller sets it with item_count_change before calling ADDITEM (GETITEM@0x109, SHOP@0x17fc, SC0060@0x1df2); ITMES compares it with each id. |
| `0x12558c` | item_count_change | high | investigation | How many of current_item ADDITEM adds, negative to remove (ADDITEM@0x8). |
| `0x3594fd` | condition_argument_id | high | investigation | The condition ADDILL and CALCILL apply (CALCILL@0x34, ADDILL@0x55); a map event sets it before calling ADDILL (MPEV01F0120@0x7b5: 7). The cell before ILINIT's arrays. |
| `0x3594fe` | condition_argument_levels | high | investigation | The levels ADDILL and CALCILL add to condition_argument_id, negative to take them away (MPEV01F0120@0x7ba: 5). |
| `0x35a784` | routine_in_play | high | investigation | The routine RTN follows: EBINIT's G[0xb53c5 + 9 * unit + column], the column the character's tactic G[0x8e36e] for a party battler and 0 for an enemy (BTL@0x13b0..0x1411). |
| `0x35a785` | routine_step_in_play | high | investigation | The routine step whose rule script runs (RTN@0x1bf). |
| `0x35a786` | routine_acted | high | investigation | Set by a rule script that acts; it ends RTN's walk (RTN@0x1ce), and RTN000 runs when it stays 0 (RTN@0x1fa). |
| `0x386a6d` | scene_run_mode | high | investigation | How SYSTEM4 plays a scene. 0, the story: SYSTEM4@0x2f9 records scene_seen_flags and @0x44a reads scene_script_ids. 1, TITLE@0x678's SC????.BIN pick: SC0000@0x475 takes its debug path. -1, DEBUGADV and DEBUGADV2 (TITLE@0x69e, 0x6bc): neither. -2, 特別編 (GAMESTART@0x3b8, CALLBACK_LOAD@0xc): the table is read, nothing recorded. 100 scripts reset it to 0. Above the saved prefix. Values: -2 特別編, -1 debug ADV, 0 story, 1 debug script. |
| `0x38a9f8` | battle_animation_in_play | high | investigation | The BTANINIT2 animation ANIME plays, from ITINIT's G[0x34f8bd] for the action G[0x952a3] (BTL@0x1cb0, @0x212e, @0x4a32); 0 while the option G[0x9050f] is set (ANIME@0x7). |
| `0x3acd40` | profile_shown | high | investigation | The CIINIT profile the 人物 book shows (INFO2@0x00de7); CIMES@0x00006 branches on it and writes its text, which INFO2@0x00eae runs. Not saved. |
| `0x3aced1` | glossary_current_entry | high | investigation | The glossary entry INFO1 shows: INFO1@0xe89 sets it, INFO1@0xe9c..0xeee draws its picture, and VIMES@0x6 branches on it to show its text. |

## story-flag

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x470` | story_flags | high | investigation | Saved: the game's flags, by scene. Each scene's own code sets [scene] to 1 at its start (SC0000@0x7b5 for scene 0; 649 of the 701 SCINIT scenes). SCJUMP's rules test them (144 of the 184 ask that their own scene's is not 1), as do $1$SCJUMP@0x57 and AMINIT ([9999], which nothing writes). No scene has an id from 1900 to 1999: [1900 + item] is a key item held, item_key_held_flags (ADDITEM@0x78..0xe7). |
| `0xbdc` | item_key_held_flags | high | investigation | Saved: for a key item (item_classes 1, ids 1..35), 1 while it is held and 0 when not; ADDITEM@0x78..0xe7 computes the cell as 0x470 + 0x76c + item. |
| `0x83c1a` | treasure_opened | high | investigation | Saved: 1 once a treasure is opened, keyed by treasure id; GETTREASURE gives nothing for an opened one and sets it before calling GETITEM (GETTREASURE@0x48). |
| `0x88a3a` | item_obtained_flags | high | investigation | Saved: 1 once the item was obtained; ADDITEM@0x62..0x75 sets it and stores it in the shared profile. |
| `0x8b226` | formation_learned_mask | high | investigation | Saved: bit f set once formation f is learned. ADDITEM@0xae..0xd1 sets bit (item − 12) for the 陣形の書, items 13..20; GAMESTART@0x687 starts with bit 1; CHMODE4@0x1541 tests it. |

## string-table

| address | name | conf | source | usage |
|---|---|---|---|---|
| `GS 0xde` | current_place_name | high | investigation | Saved: the place name. ALLMAP@0x50 sets 全体マップ, TOWN@0x28 and FIELD@0x159 copy area_names[current_place_id], FIELD@0x170 copies the floor's name from map_floor_names. What reads it was not traced. |
| `GS 0xe0` | speaker_names | high | investigation | CNINIT: the speaker's name plate (SC0470@0x00b81, HISTORY@0x0105f); the table's leader. |
| `GS 0x616` | unit_full_names | med | investigation | EBINIT: the full name (セリカ・シルフィル). No bg0 script reads it. |
| `GS 0x9fe` | unit_names | high | investigation | EBINIT: the name the game shows (331 scripts; INFO3@0x01011). |
| `GS 0xde6` | unit_epithets | high | investigation | EBINIT: the epithet shown with the name (バリハルト魔法剣士; CHANGECH, CHMODE1 to CHMODE4, DRAWCHP). |
| `GS 0x11ce` | unit_description_lines | high | investigation | EBINIT: two lines on how the unit fights, drawn by the bestiary (INFO3@0x01185, @0x01199) and DRAWENP. |
| `GS 0x199e` | area_names | high | investigation | AMINIT: the area's name (廃都ノヒア, マクルの街 ...), copied into current_place_name by TOWN@0x28 and FIELD@0x159. AMINIT overwrites it but does not clear it. |
| `GS 0x1a02` | town_site_names | med | investigation | SIINIT: the site's name (街を出る, 大聖堂, ラギールの店 ...); site 1 is always the exit. No script indexes it. |
| `GS 0x1a20` | arena_round_names | high | investigation | ARINIT: the round's title (タイトル), drawn in ARENA's list (ARENA@0xef0). |
| `GS 0x1a48` | arena_round_texts | high | investigation | ARINIT: column 0 the condition (条件) in the list, red while the round is closed (ARENA@0xf04..0xf2d); columns 1 to 3 the description lines of the round under the cursor (ARENA@0x1288..0x12cc); column 3 is never written. Columns: 0=condition, 1=description_1, 2=description_2, 3=description_3. |
| `GS 0x1ae8` | map_floor_names | high | investigation | MPINIT: each floor's name, columns 1 to 9; FIELD@0x170 copies it into current_place_name, and BTL@0x559a reads it. |
| `GS 0x2e70` | item_names | high | investigation | ITINIT: the item's name (742 written, 嵐神の鍵 …, 9999 素手); ARENA, BTL, CHMODE1, CHMODE3, DRAWCHP, DRAWENLIST, GETITEMMES, INFO4, RESULT and SHOP draw it. |
| `GS 0x5580` | item_notes | med | investigation | ITINIT: an author's note on where 13 key items come from (13 基本陣形の書 最初から, 14 ダルノスと二人で鉄屑の谷に行くところ); no reader found. |
| `GS 0x7c90` | condition_names | high | investigation | ILINIT: the name of each level, GS[0x7c90 + 5*cond + level − 1] (呪い1 to 呪い5; 即死, 喪意, 石化 one level); BTL, CHANGECH, DRAWCHP, DRAWENP draw it. |
| `GS 0x7d58` | condition_descriptions | high | investigation | ILINIT: up to 10 description lines a condition (猛毒 "戦闘中は、効果時間中1フレーム毎に…"); no reader found by this base. |
| `GS 0x7ee8` | skill_support_names | high | investigation | SSINIT: the names of the 12 support effects (支援効果・祈り …); CHMODE2@0x2651 draws the current character's, their ids from G[0xabb6d + kind] and G[0xabf55 + 5*kind + k] (CHMODE2@0x1205, @0x124a). |
| `GS 0x7f4c` | skill_support_descriptions | high | investigation | SSINIT: two description lines a support effect (10フレーム毎にHPが回復 …); CHMODE2@0x2661..0x2677 joins them. |
| `GS 0x8014` | element_attack_names | high | investigation | AFINIT: the attack element names 1..17 (物理, 万能, 火炎, 冷却, 電撃, 地脈, 神聖, 暗黒, 戦意, 10 blank, 抵抗1..7); CHMODE3 and DRAWCHP draw them. No 無属, unlike Verita. |
| `GS 0x8028` | element_defense_names | high | investigation | AFINIT: the defense element names 1..14 (1..8 as the attack elements, 神格位, 魔神, 反万能, 霊体, 不死, 創造物); DRAWCHP, DRAWENP, INFO3 draw them. |
| `GS 0x803c` | item_class_names | high | investigation | AFINIT: the class names (1 重要アイテム … 10 装飾品, 21 魔術・治癒 … 35 魔術・招聘, 40 敵魔術, 41 必殺・飛燕 … 46 必殺・盗賊, 50 敵必殺); CHMODE2 draws them. |
| `GS 0x8078` | item_class_short_names | high | investigation | AFINIT: the short class names (重要, 物品, 非戦, 小型 …, 召喚Ａ, 召喚Ｂ, 招聘 …). |
| `GS 0x80b4` | item_class_equipment_notes | high | investigation | AFINIT: the equipment note of a class: 6 「盾」装備不可, 41..43 the weapons the art school needs; CHMODE2 draws them. |
| `GS 0x80f0` | formation_names | high | investigation | AFINIT: the formation names 1..8 (基本陣形 … 魔術陣形); CHMODE4@0x1c42 draws the party's. |
| `GS 0x80f9` | formation_effect_lines | med | investigation | AFINIT: a formation's six effect lines (攻撃陣形: 物理攻撃up, 物理防御down, 物理防御up, 物理攻撃down, then two description lines); CHMODE4@0x1ca1. Columns: 0=front_up, 1=front_down, 2=rear_up, 3=rear_down, 4=description_1, 5=description_2. |
| `GS 0x8177` | profile_names | high | investigation | CIINIT: the profile's name (INFO2@0x00c45, @0x00c8b). |
| `GS 0x81db` | glossary_titles | high | investigation | VIINIT: the entry's title (【用語】二つの回廊の終わり ...), ids 1 to 90. INFO1@0xd2d lists it. |
| `GS 0xdc` | — | med | auto-shape | string-table (written by DEBUGADV) |
| `GS 0xdd` | — | med | auto-shape | string-table (written by DEBUGADV) |
| `GS 0xe1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xe2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xe3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xe4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xea` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xeb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xed` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xee` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xef` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xf9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0xfa` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x104` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x105` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x108` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x10a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x10b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x10c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x10d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x10e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x10f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x110` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x111` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x112` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x113` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x114` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x115` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x116` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x117` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x118` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x119` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x11c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x11d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x11e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x11f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x120` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x121` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x122` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x123` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x124` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x125` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x126` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x127` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x128` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x129` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x12a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x12b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x12c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x12d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x12e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x12f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x130` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x131` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x132` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x133` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x134` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x13e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x13f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x140` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x141` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x142` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x143` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x144` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x145` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x147` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x148` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x149` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x14a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x14b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x14e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x14f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x150` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x153` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x154` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x157` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x158` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x159` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x15d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x15e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x15f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x160` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x162` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x163` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x164` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x165` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x166` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x167` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x168` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x16c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x176` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x179` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x17a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x17b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x17d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x17e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x17f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x180` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x181` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x182` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x183` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x184` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x185` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x186` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x187` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x18a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x18b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x18c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x18d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x18e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x18f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x190` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x191` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x192` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x193` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x194` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x195` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x196` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x197` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x198` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x199` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x19a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x19b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x19c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x19d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x19e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x19f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1a8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ad` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1af` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1bc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1bd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1be` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ce` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cf` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1df` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ea` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1eb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1fd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x210` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x21b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x234` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x236` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x237` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x238` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x240` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x243` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x248` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x249` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x270` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x271` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x298` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x299` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ac` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ad` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ae` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2af` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ba` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2da` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2db` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2dc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2dd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2de` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2df` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x370` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x371` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x372` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x379` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x37a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x388` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x39c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x39d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x39e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x39f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3a0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3a1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3a2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3a3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3a4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ab` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ec` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ed` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ee` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ef` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x400` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x40a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x40b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x40c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x40e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x40f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x414` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x432` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x433` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x434` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x437` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x438` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x43a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x43c` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x464` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x465` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x466` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x467` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x468` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x469` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x472` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x473` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x474` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x475` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x476` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x477` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x478` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x479` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x47a` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x47b` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x482` | — | med | auto-shape | string-table (written by CNINIT) |
| `0x496` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x5a5` | — | med | auto-shape | string-table (written by MPEV18B0113) |
| `GS 0x614` | — | med | auto-shape | string-table (written by SAVE) |
| `0x615` | — | med | auto-shape | string-table (written by ARENA) |
| `0x617` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x618` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x619` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x61a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x620` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x621` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x623` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x624` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x625` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x626` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x627` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x629` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x62a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x62b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x62c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x62d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x62e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x62f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x630` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x63a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x63b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x63e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x640` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x641` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x642` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x643` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x644` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x645` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x646` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x647` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x648` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x649` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x64a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x64b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x64c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x64d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x64e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x64f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x652` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x653` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x654` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x655` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x656` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x657` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x658` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x659` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x65a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x65b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x65c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x65d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x65e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x65f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x660` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x661` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x662` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x663` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x664` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x665` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x666` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x667` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x668` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x669` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x66a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x674` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x675` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x676` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x677` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x678` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x679` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x67a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x67b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x67d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x67e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x67f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x680` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x681` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x684` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x685` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x686` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x689` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x68a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x68d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x68e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x68f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x693` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x694` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x695` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x696` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x698` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x699` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x69a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x69b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x69c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x69d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x69e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6a2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6af` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6b3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b7` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6b9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6ba` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6bb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6c9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6ca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x6db` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6de` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6f4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6f9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x6fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x702` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x703` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x704` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x705` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x706` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x707` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x70a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x70f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x715` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x71a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x71b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x71c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x71d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x720` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x721` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x733` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x743` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x744` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x746` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x751` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x763` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x764` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x76a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x76c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x76d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x76e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x774` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x775` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x776` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x779` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x77e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x77f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x780` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7a7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7b2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7b3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7b4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7ed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7ee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7ef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x7f0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x7f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x7f7` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x7f9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x7fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x7fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x7fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x80b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x80c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x80d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x80e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x80f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x810` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x811` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x812` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x813` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x814` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x815` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x816` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x817` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x818` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x819` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x81a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x81b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x81c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8be` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x8e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x918` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x919` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x91a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x91b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x91c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x91d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x91e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x922` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x923` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x924` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x925` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x926` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x927` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x928` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x92b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x92c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x92d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x92e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x936` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x940` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x941` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x942` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x944` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x945` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x94a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x968` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x969` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x96a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x96d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x96e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x970` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x972` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x99a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x99b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x99c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x99d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x99e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x99f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9ad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9b8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x9ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa01` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa08` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa09` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa0b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa0c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa0d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa0e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa0f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa11` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa12` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa13` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa14` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa15` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa16` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa17` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa18` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa22` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa23` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa26` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa28` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa29` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa2a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa2b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa2d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa2e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa2f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa30` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa31` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa32` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa33` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa34` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa35` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa36` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa37` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa3a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa3b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa3c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa3d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa3e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa3f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa40` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa41` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa42` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa43` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa44` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa45` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa46` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa47` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa48` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa49` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa4a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa4b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa4c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa4d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa4e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa4f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa50` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa51` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xa52` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa5c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa5d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa5e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa5f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa60` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa61` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa62` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa63` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa65` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa66` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa67` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa68` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa69` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa6c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa6d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa6e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa71` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa72` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa75` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa76` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa77` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa7b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa7c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa7d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa7e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa80` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa81` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa82` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa83` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa84` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa85` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa86` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa8a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa94` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa9b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa9d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xa9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaa9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaaa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaaf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xab9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xabb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xabc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xabd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xabe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xabf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xac6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xacb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xacd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xada` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xadb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xadc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xade` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xae1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xae2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xae3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaeb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaf2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xaf7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xafd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb04` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb05` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb08` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb09` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb1b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb2b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb2e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb39` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb4b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb4c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb52` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb54` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb55` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb56` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb5c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb5d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb5e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb61` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb66` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb67` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xb68` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb8e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb8f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb9a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb9b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xb9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbb6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbb7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbb8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbb9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbbb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbbc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbcb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbcc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbcd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbd4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbd5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbd6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbd7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbd8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xbde` | — | med | auto-shape | string-table (written by EBINIT) |
| `0xbdf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbe1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbe2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbe3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbe4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbf9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbfa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbfb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbfc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbfd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbfe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xbff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc01` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc04` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc88` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc8b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc8c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc8d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc8e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc8f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xc98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xca6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcbb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcbc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcbd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcbe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcbf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcc0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcc1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcc2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xcc9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd01` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd04` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd05` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd06` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd0a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd0b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd0c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd0d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd0e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd0f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd10` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd13` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd14` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd15` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd16` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd1e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd28` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd29` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd2a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd2d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd32` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd50` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd51` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd52` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd55` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd56` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd58` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd5a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd82` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd83` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd84` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd85` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd86` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd87` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd91` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd92` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd93` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd94` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd95` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd96` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xd99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xda0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdb4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xde7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xde8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xde9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdf9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdfa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdfb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdfc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdfd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdfe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xdff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe0a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe0b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe0e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe10` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe11` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe12` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe13` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe14` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe15` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe16` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe17` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe18` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe19` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe1a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe1b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe1c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe1d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe1e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe1f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe22` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe23` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe24` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe25` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe26` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe27` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe28` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe29` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe2a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe2b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe2d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe2e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe2f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe30` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe31` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe32` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe33` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe34` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe35` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe36` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe37` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe38` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe39` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe3a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe44` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe45` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe46` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe47` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe48` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe49` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe4a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe4b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe4d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe4e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe4f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe50` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe51` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe54` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe55` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe56` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe59` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe5a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe5d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe5e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe5f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe63` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe64` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe65` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe66` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe68` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe6c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe6e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe72` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe7c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe7f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe80` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe81` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe83` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe84` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe85` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe86` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe87` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe88` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe8a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe8b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe8c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe8d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe91` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe92` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe93` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe94` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe95` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe96` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe9a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe9b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe9d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xe9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xea9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeaa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xead` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeb3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeb5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xec2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xec3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xec4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xec6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xec9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xecb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xed2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xed3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xed4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xed5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xed6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xed7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeda` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xedf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xee5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeeb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xeed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xef0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xef1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf13` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf14` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf16` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf21` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf33` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf34` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf3a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf3c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf3d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf3e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf44` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf45` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf46` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf49` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf4e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf4f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf50` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf76` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf77` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf80` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf81` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf82` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf83` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf84` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xf9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfa0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfa1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfa2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfa3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfa4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfb2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfb3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfb4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfb5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfb6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfbc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfbd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfbe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfbf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfc0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfc6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfc7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfc9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfcb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfcc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfdb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfdc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfdd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfde` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfdf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfe9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfeb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0xfec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1070` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1073` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1074` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1075` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1076` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1077` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1078` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x107f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1080` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x108e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10ed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10f8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10fd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x10fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1106` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1110` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1111` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1112` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1114` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1115` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x111a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1138` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1139` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x113a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x113d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x113e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1140` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1142` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x116a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x116b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x116c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x116d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x116e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x116f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1178` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1179` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x117a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x117b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x117c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x117d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x117e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x117f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1180` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1181` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1188` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x119c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11ed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11f4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x11ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1228` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1229` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x122a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x122b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x122c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x122d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x122e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1230` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1234` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1236` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1238` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1239` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x123e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1240` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1241` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1256` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1257` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x128a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x128c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x128e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1290` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1292` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1293` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1296` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1297` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1298` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1299` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x129c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x129d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x129e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12de` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12df` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1300` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1301` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1302` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1304` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1305` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1308` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1309` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1310` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1311` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1312` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1313` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1316` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1317` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1318` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1319` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x135e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x135f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1368` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1369` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x136c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x136d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1386` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1387` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1388` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1389` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x138a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x138e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x138f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1394` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1395` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1396` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1398` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1399` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1408` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1409` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1428` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1429` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x142a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x142e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x142f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1444` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1445` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1468` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1469` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x146a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x146b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1476` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1477` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x147a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x147b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x147c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x147e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x148a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x148b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x148c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x148e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x148f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1494` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1495` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x149e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x149f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14a0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14a1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14a2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14a3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1502` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1504` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1506` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1508` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x150a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x153e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x153f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1540` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1542` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1543` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1544` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1545` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1546` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1547` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1548` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1549` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x154a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1566` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1567` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1568` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1569` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x156a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x156b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x156c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x156d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x156e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x156f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x157a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x157b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x157c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x157e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x157f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1580` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1581` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1582` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1583` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x158e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x158f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1590` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1594` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1595` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15b8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15be` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15bf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15c9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x15db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1700` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1701` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1702` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1703` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1746` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1747` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1750` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1751` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1752` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1754` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1756` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1757` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17f0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17f1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17f8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17f9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17fd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x180e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1822` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1823` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1824` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1826` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1827` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1836` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1837` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1872` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1873` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1874` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1875` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1876` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1877` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x187c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x187d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x187e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x187f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1882` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1883` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1886` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1887` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18de` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18df` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18f8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1900` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1901` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1902` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1903` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1904` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1905` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1912` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1913` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x193a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x199f` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a0` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a1` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a2` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a3` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a4` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a5` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a6` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a7` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a8` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19a9` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19aa` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19ad` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19ae` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19af` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b0` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b1` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b3` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b4` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b5` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b6` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b7` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b8` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19b9` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19ba` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19bb` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19bc` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19bd` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19c1` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19c2` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19c3` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19c4` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19c5` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19c7` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19d0` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19d1` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19d2` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19da` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19db` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19dc` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19e4` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19ee` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19ef` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19f0` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x19f8` | — | med | auto-shape | string-table (written by AMINIT) |
| `GS 0x1a03` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a04` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a05` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a06` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a07` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a08` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a09` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a0a` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a0b` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a0c` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a0d` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a0e` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a0f` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x1a21` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a22` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a23` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a24` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a25` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a26` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a2a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a2b` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a2c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a2d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a2e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a2f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a30` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a34` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a35` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a36` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a37` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a38` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a39` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a3a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a3e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a3f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a40` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a41` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a42` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a43` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a44` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a49` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a4a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a4c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a4d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a4e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a50` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a51` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a52` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a54` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a55` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a56` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a58` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a59` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a5a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a5c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a5d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a5e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a60` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a61` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a62` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a70` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a71` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a72` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a74` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a75` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a76` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a78` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a79` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a7a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a7c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a7d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a7e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a80` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a81` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a82` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a84` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a85` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a86` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a88` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a89` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a8a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a98` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a99` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a9a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a9c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a9d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1a9e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa2` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa4` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa6` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aa9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aaa` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aac` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aad` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aae` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ab0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ab1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ab2` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac2` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac4` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac6` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ac9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1aca` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1acc` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1acd` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ace` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad2` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad4` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad6` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ad9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1ada` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x1b4d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1b4e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1b4f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1bb1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1bb2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1bb3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1bb4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1bb7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1c15` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1c79` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1cdd` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1cde` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1d41` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1da5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1da6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1da7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1da8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1e09` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1e0a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1e0b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1e0c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1ed1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f35` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f99` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f9a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f9b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f9c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f9d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x1f9e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x20c5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2129` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x218d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x21f1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x21f2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2255` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x231d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x231e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2381` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x23e5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2449` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x244a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x244b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x244c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x244d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x244e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x244f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x24ad` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2511` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2575` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x25da` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x263d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x26a1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x26a2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x26a3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x26a4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2705` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2706` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2707` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2895` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2896` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x28f9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x28fa` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x295d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x29c1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2a25` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2aed` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2aee` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e73` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e74` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e75` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e76` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e77` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e78` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e79` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e7a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e7b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e7c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e7d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e7e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e7f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e80` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e81` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e82` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e83` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e84` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e85` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e86` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e87` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e88` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e89` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e8a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e8b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e8c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e8d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e8e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e8f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e90` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e91` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e92` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2e93` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ed5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ed6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ed7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ed8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ed9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eda` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2edb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2edc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2edd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ede` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2edf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ee9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eeb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2eef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ef7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f07` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f08` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f09` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f0a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f0b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f0c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f0d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f0e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f0f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f10` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f11` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f12` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f13` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f14` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f15` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f16` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f39` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f3a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f3b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f3e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f3f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f40` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f43` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f44` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f45` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f48` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f49` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f4a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f4d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f4e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f4f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f52` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f53` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f54` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f57` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f58` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f5c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f5d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f61` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f62` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f6c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f6d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f6e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f6f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f70` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3259` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x325a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x325b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x325c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x325d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x325e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x325f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3260` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3261` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3262` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32bd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32be` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32bf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x32c8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3321` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3322` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3323` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3324` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3325` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3326` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3327` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3328` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3329` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x332a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x332b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x332c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x332d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x332e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x332f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3330` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3352` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3385` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3386` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3387` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3388` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3389` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x338a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x338b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x338c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x338d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x338e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x338f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3390` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3391` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3392` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33eb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33f0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x344d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x344e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x344f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3450` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3451` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34b0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34b1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34b2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34b3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34b4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3515` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3516` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3517` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3518` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3519` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x351a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x351b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x351c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3641` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3642` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3643` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3644` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x364b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x364c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x364d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x364e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x364f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3650` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3651` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3652` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3655` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3656` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3657` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x365f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3660` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3661` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3669` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x366a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x366b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3673` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3674` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3675` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x367d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x367e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x367f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3680` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3687` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3688` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3689` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x368a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3691` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x369b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x36a5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x36af` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x36b0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3709` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x370a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x370b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x370c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3713` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3714` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3715` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3716` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x371d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x371e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x371f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3727` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3728` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3729` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3731` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3732` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3733` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x373b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x373c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x373d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3745` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3746` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3747` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3750` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3751` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3759` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x375a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x375b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x378b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3956` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3957` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3958` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3959` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x395a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x395b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x395c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x395d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x395e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x395f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3961` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3962` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3963` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3964` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3965` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3966` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3967` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3968` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3969` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x396a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39c5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39c6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39c7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39c8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39c9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39ca` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39cb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39cc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39cd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39ce` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39cf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39d9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39da` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39db` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39dc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39dd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39de` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39df` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x39e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a29` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a2a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a2b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a2c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a2d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a2e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a2f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a33` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a34` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a35` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a36` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a37` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a38` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a39` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a3d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a3e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a3f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a40` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a41` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a42` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a43` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a47` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a48` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a49` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a4a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a4b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a4c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a4d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a51` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a52` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a53` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a54` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a55` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a56` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a57` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a5b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a5c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a5d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a5e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a5f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a60` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a61` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a65` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a66` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a67` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a68` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a69` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a70` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a73` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a74` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a75` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a79` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a7a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a7b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a7c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a7d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a7e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a7f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a83` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a84` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a85` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a86` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a87` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a88` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a89` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a8d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a8e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a8f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a90` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a91` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a92` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a93` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a97` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a98` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a99` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a9a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a9b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a9c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a9d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aa8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aae` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aaf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ab0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ab1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e11` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e12` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e13` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e14` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e15` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e16` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e17` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e1b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e1c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e1d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e1e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e1f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e20` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e21` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e25` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e26` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e27` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e28` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e29` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e2a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e2b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e2f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e30` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e31` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e32` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e33` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e34` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e35` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e39` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e3a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e3b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e3c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e3d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e3e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e3f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e43` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e44` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e45` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e46` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e47` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e48` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3e49` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41f9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41fa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41fb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41fc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41fd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41fe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x41ff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4200` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4201` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4202` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4203` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4204` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4205` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4206` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4207` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4208` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4209` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x420a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x420b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x420c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x420d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x420e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x420f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4210` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4211` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4212` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4213` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4214` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4215` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4216` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4217` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4218` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4219` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x421a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x421b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x421c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x421d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x421e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4220` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4221` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4222` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4223` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4224` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4225` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4226` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4227` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4228` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4229` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x422a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x422b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x422c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x422d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x422e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x422f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4230` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4231` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4232` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4233` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4234` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4235` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4236` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4237` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4238` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4239` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x423a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x423b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x423c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x423d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x423e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x423f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4240` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4241` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4242` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4243` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4244` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4245` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4246` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4247` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4248` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4249` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x424a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x424b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x424c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x425c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x425d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x425e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x425f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4260` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4261` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4262` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4263` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4264` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4265` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45ec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45ef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45f9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45fa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45fb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45fd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45fe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x45ff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4601` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4602` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4603` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4604` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4605` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4608` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4609` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x460a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x463f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4640` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4641` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4642` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4643` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4644` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4645` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4646` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4647` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4648` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4649` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x464a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46aa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49c9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49cb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49cc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49cd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49d4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49d5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49d6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49d7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49d8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49d9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49da` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49db` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49df` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49e0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49e6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49e7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49e8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49ee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49ef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49f0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49f1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49f2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49f9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49fa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49fb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49fc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49fd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49fe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x49ff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a00` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a01` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a02` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a03` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a04` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a05` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a06` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a07` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a08` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a09` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a0a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a0b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a0c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a0d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a0e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a0f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a10` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a11` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a12` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a13` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a14` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a15` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4a16` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5578` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5579` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x557a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x557b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x557c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x557d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x557e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x557f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x558d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x558e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x558f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5590` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5591` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5592` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5593` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5594` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x559c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x559d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x559e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x559f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x55a0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x7c95` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7c9a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7c9f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ca4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ca5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ca6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ca7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ca8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ca9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7caa` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cab` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cac` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cad` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cae` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7caf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cb9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cba` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cbb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cbc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cbd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cbe` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cbf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cc9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cca` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ccb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ccc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ccd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cce` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ccf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cd9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cda` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cdb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cdc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cdd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cde` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cdf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ce9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cea` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ceb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cec` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ced` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cee` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cef` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cf9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cfa` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cfb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cfc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cfd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cfe` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7cff` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d00` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d01` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d02` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d03` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d04` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d05` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d06` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d07` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d08` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d09` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d0a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d0b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d0c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d0d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d0e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d0f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d10` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d11` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d12` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d13` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d14` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d15` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d16` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d17` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d18` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d19` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d1a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d1b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d1c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d1d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d1e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d1f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d20` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d21` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d22` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d23` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d24` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d25` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d26` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d27` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d28` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d29` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d2a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d2b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d2c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d2d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d2e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d2f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d30` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d31` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d32` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d33` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d34` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d35` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d36` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d37` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d38` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d39` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d3a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d3b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d3c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d3d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d3e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d3f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d40` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d41` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d42` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d43` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d44` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d45` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d46` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d47` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d48` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d62` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d6c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d76` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d80` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d82` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d84` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d86` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d8a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d8c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d8e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d94` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d96` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d98` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7d9e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7da0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7da8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7daa` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dbc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dda` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ddc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dee` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfa` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfe` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e00` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e02` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e04` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e10` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e16` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e18` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e20` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e22` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e34` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e36` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e40` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e48` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e52` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e54` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e66` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e68` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e70` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e72` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e7a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e7c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e84` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e86` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e8e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e90` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e98` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e9a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ea2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ea4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7eac` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7eae` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7eb6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7eb8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7eba` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ec0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ec2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ee9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7eea` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7eeb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7eec` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7eed` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7eee` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7eef` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7ef0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7ef1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7ef2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7ef3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7ef4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f4e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f50` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f52` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f54` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f56` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f58` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f59` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f5a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f5c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f5e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f60` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f62` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x7f64` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8015` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8016` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8017` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8018` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8019` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x801a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x801b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x801c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x801d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x801f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8020` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8021` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8022` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8023` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8024` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8025` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8029` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x802a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x802b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x802c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x802d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x802e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x802f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8030` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8031` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8032` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8033` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8034` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8035` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8036` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x803d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x803e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x803f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8040` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8041` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8042` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8043` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8044` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8045` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8046` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8051` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8052` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8053` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8054` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8055` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8056` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8057` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8058` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8059` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x805a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x805b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x805c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x805d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x805e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x805f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8064` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8065` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8066` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8067` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8068` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8069` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x806a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x806e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8079` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x807a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x807b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x807c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x807d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x807e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x807f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8080` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8081` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8082` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x808d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x808e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x808f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8090` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8091` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8092` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8093` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8094` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8095` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8096` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8097` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8098` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8099` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x809a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x809b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a0` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a3` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a4` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80a6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80aa` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80ba` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80dd` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80de` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80df` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f3` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f4` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f7` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80f8` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x80ff` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8101` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8103` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8104` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8105` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8106` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8107` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8108` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8109` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x810a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x810b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x810c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x810d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x810e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x810f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8110` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8111` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8112` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8113` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8114` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8115` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8116` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8117` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8118` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8119` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x811a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x811b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x811c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x811d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x811e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x811f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8120` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8121` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8122` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8123` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8124` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8125` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8126` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8127` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8128` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8129` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x812a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x812b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x812c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x812d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x812e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8130` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8131` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8132` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8133` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8134` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8135` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8136` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8137` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x813f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8140` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8141` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8142` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8143` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8144` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8145` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8146` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8147` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8148` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8149` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x814a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x814b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x814c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x814d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x814e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x814f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8150` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8151` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8152` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8153` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8154` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8155` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8156` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8157` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8158` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8159` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x815a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x815b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x815c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x815d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x815e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x815f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8160` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8161` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8162` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8163` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8164` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8165` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8166` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8167` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8168` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8169` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x816a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x816b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x816c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x816d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x816e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x816f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8170` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8171` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8172` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8173` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8174` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8175` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8176` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0x8178` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8179` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x817a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x817b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x817c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x817d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x817e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x817f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8180` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8181` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8182` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8183` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8184` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8185` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8186` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8187` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8188` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8189` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x818a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x818b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x818c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x818d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x818e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x818f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8190` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8191` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8192` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8193` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8194` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8195` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8196` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8197` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8198` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x8199` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x819a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x819b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x819c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x819d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x819e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x819f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a0` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a1` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a2` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a3` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a4` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a5` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a6` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a7` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a8` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81a9` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81aa` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x81dc` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81dd` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81de` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81df` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e0` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e1` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e2` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e3` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e4` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e5` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e6` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e7` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e8` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81e9` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81ea` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81eb` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81ec` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81ed` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81ee` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81ef` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f0` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f1` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f2` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f3` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f4` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f5` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f6` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f7` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f8` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81f9` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81fa` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81fb` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81fc` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81fd` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81fe` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x81ff` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8200` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8201` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8202` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8203` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8204` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8205` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8206` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8207` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8208` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8209` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x820a` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x820b` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x820c` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x820d` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x820e` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x820f` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8210` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8211` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8212` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8213` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8214` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8215` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8216` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8217` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8218` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8219` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x821a` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x821b` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x821c` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x821d` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x821e` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x821f` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8220` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8221` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8222` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8223` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8224` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8225` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8226` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8227` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8228` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8229` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x822a` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x822b` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x822c` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x822d` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x822e` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x822f` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8230` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8231` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8232` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8233` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8234` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x8235` | — | med | auto-shape | string-table (written by VIINIT) |

## ui-toggle

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x3` | config_initialized | high | investigation | A shared-profile cell. SYSTEM4@0x1c3 loads it; while it is 0, SYSTEM4@0x1d3 runs INITCONFIG, then sets it and stores it. Otherwise LOADCONFIG reads the options back (SYSTEM4@0x1cd). |
| `0x2be7` | voice_mute_switches | high | investigation | Saved, and kept in the shared profile: a switch per voice setting (CVINIT's 0 to 15), 1 to mute it. INITCONFIG@0x3b clears them and stores them (@0x5d); the Option screen sets one and stores it (CONFIG@0xeb0..0xeea). A scene mutes the next voice when the speaker's is set, G[0x2be7 + unit_voice_settings[speaker_voice_units[adv_speaker_id]]] (SC0470@0xbd8, SC0000@0xeaa); BTL (@0x19aa), HISTORY and ANIME read it by a unit's setting. |
| `0x2fd1` | adv_page_has_text | med | investigation | 1 once a page has text: every dialogue page sets it first (142 scripts, SC0470@0x00460), and the special mode's event scripts clear it (`$1$MPEV49F0101@0x005ee`). |
| `0x2fec` | menu_cursor_auto_move_enabled | high | investigation | An option, 1 by INITCONFIG@0x10 and kept in the shared profile. While it is set, BUNKI@0x2a1 (and SBUNKI, BUNKIMOVE) put the cursor on a choice menu's first row; otherwise the cursor stays where it is (BUNKI@0x2e6). Himegari's option of the same name. |
| `0x30ae` | skip_read_messages_only | high | investigation | An option, 1 by INITCONFIG@0x33 and kept in the shared profile. With 1, a scene's skip button toggles read-message skip (SC0000@0x201..0x219); otherwise it turns message skip on, which skips every message (SC0000@0x21f). |
| `0x30af` | map_frame_rate | med | investigation | An option, 60 by INITCONFIG@0x2b and kept in the shared profile; CONFIG@0x91c and @0x935 set 60 or 30. FIELD@0xaab waits 1000 / value ms a frame and FIELD@0x1063 steps 60 / value; DRAWWALK reads it too. |
| `0x316d` | map_continue | med | investigation | Saved: 'MapContinue' by the debug text at FIELD@0x235..0x25c. When it is 0, FIELD@0x18e..0x198 copies the target position into the position; EVOLVEOBJ plays an object's change sound only when it is not 0. Its full role is not established. |
| `0x317c` | map_entered | high | investigation | Saved: 1 once FIELD has started on this map entry (FIELD@0x99); ALLMAP@0xe and TOWN@0x11 clear it. FIELD reads MPINIT's start only while it is 0 (FIELD@0xba), and CALLBACK_LOAD@0xbc..0xde reruns RMINIT for a save made with it set. |
| `0x9050e` | confirm_escape | high | investigation | An option, 1 by INITCONFIG@0x8 and kept in the shared profile. While it is set, MENU@0x50d asks 脱出する / 脱出しない (SBUNKI) before the escape command runs SCJUMP's context 6. |
| `0x9100e` | in_room | med | investigation | ROOM@0x0 sets it when the extras room opens (CGMODE, HMODE, MMODE, INFO, HOSOKU); ROOM ends with exit-script (ROOM@0x627), the soft reset that clears it. While it is set, a scene's header registers no right-click menu (SC0000@0x158). |
| `0x91052` | scene_over_map | med | investigation | A scene is playing over the map. A map event (MPEV01B0102@0x1d0, 128 MPEV scripts), HOSOKU and MENU@0x5dc set it around their inline scene pass and clear it after (MPEV01B0102@0x49a). While it is set, the scene pass erases only graphic elements 10000..49999 instead of every retained object (SYSTEM4@0x34a), and SYSTEM4@0x435 copies another inline array into G[0x2fd3]. |
| `0x386a6f` | debug_map_mode | med | investigation | Set by DEBUGMAP@0x8, DEBUGMAP2@0x187 and DEBUGMAP3@0x8a. FIELD@0x128 runs no SCJUMP while it is set; INFO1@0x8ec unlocks every glossary entry; BTL and BTLMENU read it too. Above the saved prefix. |

## unknown

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x4` | — | med | auto-shape | record-table[stride 5] |
| `0x22a` | — | med | auto-shape | record-table[stride 5] |
| `0x2fed` | — | med | auto-shape | record-table[stride 3] |
| `0x3376` | — | med | auto-shape | record-table[stride 200] |
| `GS 0x8138` | — | med | auto-shape | record-table[stride 7] |
| `0x1ba16` | — | med | auto-shape | record-table[stride 40] |
| `0x20836` | — | med | auto-shape | record-table[stride 100] |
| `0x5d8c6` | — | med | auto-shape | record-table[stride 313] |
| `0x8c42e` | — | med | auto-shape | record-table[stride 40] |
| `0x8d3ce` | — | med | auto-shape | record-table[stride 40] |
| `0x8e5c6` | — | med | auto-shape | record-table[stride 40] |
| `0x90512` | — | med | auto-shape | record-table[stride 9] |
| `0x90896` | — | med | auto-shape | record-table[stride 9] |
| `0x9101e` | — | low | auto-shape | index/counter? |
| `0x91022` | — | low | auto-shape | index/counter? |
| `0x9524e` | — | low | auto-shape | index/counter? |
| `0x9524f` | — | low | auto-shape | index/counter? |
| `0x952a3` | — | low | auto-shape | index/counter? |
| `0xabf55` | — | med | auto-shape | record-table[stride 5] |
| `0xc57ac` | — | low | auto-shape | index/counter? |
| `0xc57ba` | — | low | auto-shape | index/counter? |
| `0xc5a45` | — | med | auto-shape | record-table[stride 100] |
| `0xca865` | — | med | auto-shape | record-table[stride 100] |
| `0xd4955` | — | med | auto-shape | record-table[stride 5] |
| `0xd4b49` | — | med | auto-shape | record-table[stride 5] |
| `0x386a5e` | — | med | auto-shape | record-table[stride 5] |
| `0x38937c` | — | low | auto-shape | index/counter? |
| `0x38980f` | — | med | auto-shape | record-table[stride 40] |
| `0x389cbf` | — | med | auto-shape | record-table[stride 40] |
| `0x38a16f` | — | med | auto-shape | record-table[stride 40] |
