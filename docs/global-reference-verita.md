<!-- DO NOT EDIT -- generated from vm-map/globals-verita.toml by tools/globals_build.py --build -->
# Global Variable Reference — verita (generated)

7575 globals (673 curated, 6902 auto shape-inferred). Source of truth: `vm-map/globals-verita.toml`.

## Tables

Open Maid Script tables (design spec §5.3): `table[record].field` is the field's array element.

### unit[1000]

Built by: `boot`.

EBINIT units keyed by unit id: every enemy, and the body a party character fights with (character_unit_ids).

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | unit_names | `GS 0x168c` | `string[1000]` | |
| full_name | unit_full_names | `GS 0x12a4` | `string[1000]` | |
| description | unit_description_lines | `GS 0x1a74` | `string[1000][2]` | |
| voice_clips | unit_voice_clips | `0x370817` | `int[1000][30]` | |
| battle_sprite | unit_battle_sprites | `0x377d47` | `int[1000]` | pictures |
| book_picture | unit_book_pictures | `0x37812f` | `int[1000]` | pictures |
| book_sort_key | unit_book_sort_keys | `0x378517` | `int[1000]` | |
| sex | unit_sexes | `0x3788ff` | `int[1000]` | |
| fs_temperament | unit_fs_temperaments | `0x378ce7` | `int[1000]` | |
| overlay_kind | unit_overlay_kinds | `0x3794b7` | `int[1000]` | |
| boss_class | unit_boss_classes | `0x37989f` | `int[1000]` | |
| width | unit_widths | `0x379c87` | `int[1000]` | |
| height | unit_heights | `0x37a06f` | `int[1000]` | |
| defense_element | unit_defense_elements | `0x37a457` | `int[1000]` | |
| defense_element_level | unit_defense_element_levels | `0x37a83f` | `int[1000]` | |
| weapon | unit_initial_weapons | `0x37ac27` | `int[1000]` | `item` |
| shield | unit_initial_shields | `0x37b00f` | `int[1000]` | `item` |
| armor | unit_initial_armors | `0x37b3f7` | `int[1000]` | `item` |
| accessory | unit_initial_accessories | `0x37b7df` | `int[1000]` | `item` |
| equip_kinds | unit_equip_kind_flags | `0x37bbc7` | `int[1000]` | |
| skill_levels | unit_skill_levels | `0x3805ff` | `int[1000][600]` | |
| lead_growth_row | unit_lead_growth_rows | `0x412dbf` | `int[1000]` | `lead_growth` |
| base_level | unit_base_levels | `0x4131a7` | `int[1000]` | |
| max_level | unit_max_levels | `0x41358f` | `int[1000]` | |
| level_catchup_divisor | unit_level_catchup_divisors | `0x413977` | `int[1000]` | |
| accuracy | unit_accuracies | `0x413d5f` | `int[1000]` | |
| evasion | unit_evasions | `0x414147` | `int[1000]` | |
| max_hp | unit_max_hp | `0x41a6d7` | `int[1000]` | |
| max_mp | unit_max_mp | `0x41aabf` | `int[1000]` | |
| max_tp | unit_max_tp | `0x41aea7` | `int[1000]` | |
| attack | unit_attacks | `0x41452f` | `int[1000]` | |
| defense | unit_defenses | `0x414917` | `int[1000]` | |
| magic_attack | unit_magic_attacks | `0x414cff` | `int[1000]` | |
| magic_defense | unit_magic_defenses | `0x4150e7` | `int[1000]` | |
| attack_count | unit_attack_counts | `0x4154cf` | `int[1000]` | |
| defense_count | unit_defense_counts | `0x4158b7` | `int[1000]` | |
| physical_resistance | unit_physical_resistances | `0x415c9f` | `int[1000]` | |
| mental_resistance | unit_mental_resistances | `0x416087` | `int[1000]` | |
| physical_speed | unit_physical_speeds | `0x41646f` | `int[1000]` | |
| mental_speed | unit_mental_speeds | `0x416857` | `int[1000]` | |
| luck | unit_luck | `0x416c3f` | `int[1000]` | |
| max_hp_growth | unit_max_hp_growth | `0x417027` | `int[1000]` | |
| max_mp_growth | unit_max_mp_growth | `0x41740f` | `int[1000]` | |
| max_tp_growth | unit_max_tp_growth | `0x4177f7` | `int[1000]` | |
| attack_growth | unit_attack_growth | `0x417bdf` | `int[1000]` | |
| defense_growth | unit_defense_growth | `0x417fc7` | `int[1000]` | |
| magic_attack_growth | unit_magic_attack_growth | `0x4183af` | `int[1000]` | |
| magic_defense_growth | unit_magic_defense_growth | `0x418797` | `int[1000]` | |
| attack_count_growth | unit_attack_count_growth | `0x418b7f` | `int[1000]` | |
| defense_count_growth | unit_defense_count_growth | `0x418f67` | `int[1000]` | |
| physical_resistance_growth | unit_physical_resistance_growth | `0x41934f` | `int[1000]` | |
| mental_resistance_growth | unit_mental_resistance_growth | `0x419737` | `int[1000]` | |
| physical_speed_growth | unit_physical_speed_growth | `0x419b1f` | `int[1000]` | |
| mental_speed_growth | unit_mental_speed_growth | `0x419f07` | `int[1000]` | |
| luck_growth | unit_luck_growth | `0x41a2ef` | `int[1000]` | |
| starting_fs | unit_starting_fs | `0x41b28f` | `int[1000]` | |
| max_fs | unit_max_fs | `0x41b677` | `int[1000]` | |
| first_wait | unit_first_waits | `0x41ba5f` | `int[1000]` | |
| routine_id | unit_routine_ids | `0x41be47` | `int[1000]` | `routine` |
| action_ids | unit_action_ids | `0x41c22f` | `int[1000][6]` | `item` |
| exp_curve | unit_exp_curves | `0x41d99f` | `int[1000]` | |
| exp_rate | unit_exp_rates | `0x41dd87` | `int[1000]` | |
| exp_rewards | unit_exp_rewards | `0x41e16f` | `int[1000][2]` | |
| money | unit_money | `0x41e93f` | `int[1000]` | |
| drop_items | unit_drop_items | `0x41ed27` | `int[1000][5]` | `item` |
| drop_chances | unit_drop_chances | `0x4200af` | `int[1000][5]` | |
| drop_required_flags | unit_drop_required_flags | `0x421437` | `int[1000][10]` | |
| drop_excluding_flags | unit_drop_excluding_flags | `0x423b47` | `int[1000][5]` | |
| capture_item | unit_capture_items | `0x424ecf` | `int[1000]` | `item` |
| capture_level | unit_capture_levels | `0x4252b7` | `int[1000]` | |
| voice_setting_id | unit_voice_settings | `0x268f74` | `int[1000]` | `voice_setting` |

### lead_growth[100]

Built by: `boot`.

CBINIT rows keyed by the row a unit names (unit_lead_growth_rows): the seven-parameter growth of the two leads, row 1 セリカ and row 31 リウイ.

| field | global | address | type |
|---|---|---|---|
| starting_parameters | lead_starting_parameters | `0x36fa06` | `int[100][7]` |
| cap_level_divisors | lead_parameter_cap_level_divisors | `0x36fcc2` | `int[100][7]` |
| cap_sum_divisors | lead_parameter_cap_sum_divisors | `0x36ff7e` | `int[100][7]` |
| cap_addends | lead_parameter_cap_addends | `0x37023a` | `int[100][7]` |
| parameter_costs | lead_parameter_costs | `0x3704f6` | `int[100][7]` |
| pool_rate | lead_pool_rates | `0x3707b2` | `int[100]` |

### character[100]

Built by: `boot`.

CHINIT characters keyed by character id, the id the save keeps a party member's state by.

| field | global | address | type | refers to |
|---|---|---|---|---|
| unit_id | character_unit_ids | `0xc1a739` | `int[100]` | `unit` |
| party | character_parties | `0xc1a671` | `int[100]` | |
| joining_party | character_joining_parties | `0xc1a6d5` | `int[100]` | |
| sort_key | character_sort_keys | `0xc1a79d` | `int[100]` | |
| carry_flag | character_carry_flags | `0xc1a801` | `int[100]` | |
| append_number | character_append_numbers | `0xc1a865` | `int[100]` | |

### title[2000]

Built by: `boot`.

TIINIT titles keyed by 20 x character + k; title 20 x character is the one a character joins with.

Record r belongs to record r ÷ 20 of `character`.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | title_names | `GS 0xdedc` | `string[2000]` | |
| description | title_description_lines | `GS 0xe6ac` | `string[2000][2]` | |
| order | title_orders | `0xaa108d` | `int[2000]` | |
| required_level | title_required_levels | `0xaa185d` | `int[2000]` | |
| required_title | title_required_titles | `0xaa202d` | `int[2000]` | `title` |
| required_flag | title_required_flags | `0xaa27fd` | `int[2000]` | |
| excluding_flag | title_excluding_flags | `0xaa2fcd` | `int[2000]` | |
| class | title_classes | `0xaa379d` | `int[2000]` | |
| master_id | title_master_ids | `0xaa3f6d` | `int[2000]` | `character` |
| equip_kinds | title_equip_kind_flags | `0xaa473d` | `int[2000]` | |
| special_skill_ids | title_special_skill_ids | `0xaa4f0d` | `int[2000][9]` | `item_class` |
| special_skill_levels | title_special_skill_levels | `0xaa955d` | `int[2000][9]` | |
| skill_levels | title_skill_levels | `0xaadbad` | `int[2000][600]` | |
| hp_bonus | title_hp_bonuses | `0xbd2b2d` | `int[2000]` | |
| mp_bonus | title_mp_bonuses | `0xbd32fd` | `int[2000]` | |
| tp_bonus | title_tp_bonuses | `0xbd3acd` | `int[2000]` | |
| accuracy_bonus | title_accuracy_bonuses | `0xbd429d` | `int[2000]` | |
| evasion_bonus | title_evasion_bonuses | `0xbd4a6d` | `int[2000]` | |
| attack_bonus | title_attack_bonuses | `0xbd523d` | `int[2000]` | |
| defense_bonus | title_defense_bonuses | `0xbd5a0d` | `int[2000]` | |
| magic_attack_bonus | title_magic_attack_bonuses | `0xbd61dd` | `int[2000]` | |
| magic_defense_bonus | title_magic_defense_bonuses | `0xbd69ad` | `int[2000]` | |
| magic_power_bonus | title_magic_power_bonuses | `0xbd717d` | `int[2000]` | |
| attack_count_bonus | title_attack_count_bonuses | `0xbd794d` | `int[2000]` | |
| defense_count_bonus | title_defense_count_bonuses | `0xbd811d` | `int[2000]` | |
| physical_resistance_bonus | title_physical_resistance_bonuses | `0xbd88ed` | `int[2000]` | |
| mental_resistance_bonus | title_mental_resistance_bonuses | `0xbd90bd` | `int[2000]` | |
| physical_speed_bonus | title_physical_speed_bonuses | `0xbd988d` | `int[2000]` | |
| mental_speed_bonus | title_mental_speed_bonuses | `0xbda05d` | `int[2000]` | |
| luck_bonus | title_luck_bonuses | `0xbda82d` | `int[2000]` | |
| critical_bonus | title_critical_bonuses | `0xbdaffd` | `int[2000]` | |
| cutin_style | title_cutin_styles | `0xbdb7cd` | `int[2000]` | |
| cutin_picture | title_cutin_pictures | `0xbdc76d` | `int[2000]` | pictures |
| battle_portrait | title_battle_portraits | `0xbdbf9d` | `int[2000]` | pictures |
| battle_sprites | title_battle_sprites | `0xbdcf3d` | `int[2000][10]` | pictures |
| status_portrait | title_status_portraits | `0xbe1d5d` | `int[2000]` | pictures |
| map_face | title_map_faces | `0xbe252d` | `int[2000]` | pictures |
| field_sprites | title_field_sprites | `0xbe2cfd` | `int[2000][25]` | |
| jump_sprites | title_jump_sprites | `0xbef04d` | `int[2000][25]` | |
| carry_sprite | title_carry_sprites | `0xbfb39d` | `int[2000]` | |
| extra_pictures | title_extra_pictures | `0xbfbb6d` | `int[2000][2]` | |
| voice_clips | title_voice_clips | `0xbfcb0d` | `int[2000][30]` | |
| earned | title_earned_flags | `0x16affb` | `int[2000]` | |

### speaker_entry[1000]

Built by: `boot`.

CNINIT speakers keyed by the scene's speaker id G[0x3489], the same number as the speaker's unit.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | speaker_names | `GS 0x1b6` | `string[1000]` | |
| voice_unit | speaker_voice_units | `0x268b8c` | `int[1000]` | `unit` |

### voice_setting[1000]

Built by: `boot`.

CVINIT voice settings keyed by setting id (0 to 47), the rows of the 設定 voice page.

| field | global | address | type | refers to |
|---|---|---|---|---|
| names | voice_setting_names | `GS 0x59e` | `string[1000][3]` | |
| unit_id | voice_setting_units | `0x26935c` | `int[1000]` | `unit` |
| sample | voice_setting_samples | `0x269744` | `int[1000]` | |

### profile[200]

Built by: `boot`.

CIINIT profiles keyed by profile id: the entries of the 人物 book (INFO3).

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | profile_names | `GS 0x10800` | `string[200]` | |
| unit_id | profile_unit_ids | `0xc48eff` | `int[200]` | `unit` |
| scenes | profile_scenes | `0xc48fc7` | `int[200][3]` | |
| picture | profile_pictures | `0xc4921f` | `int[200]` | pictures |
| picture_x | profile_picture_xs | `0xc492e7` | `int[200]` | |
| picture_y | profile_picture_ys | `0xc493af` | `int[200]` | |

### item[10000]

Built by: `boot`.

ITINIT records keyed by item id: items 1..999, weapons 1001.., armour 2001.., magic 3001.., arts 4001.., enemy actions 5001..9998, 9999 素手. `item[1009].name` is item_names[1009].

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | item_names | `GS 0x2f78` | `string[10000]` | |
| note | item_notes | `GS 0x5688` | `string[10000]` | |
| sort_key | item_sort_keys | `0x63df39` | `int[10000]` | |
| rank | item_ranks | `0x640649` | `int[10000]` | |
| category | item_categories | `0x642d59` | `int[10000]` | |
| class | item_classes | `0x645469` | `int[10000]` | `item_class` |
| damage_kind | item_damage_kinds | `0x647b79` | `int[10000]` | |
| once_per_battle | item_once_per_battle_flags | `0x64a289` | `int[10000]` | |
| charges | item_charge_flags | `0x64c999` | `int[10000]` | |
| attack_element | item_attack_elements | `0x64f0a9` | `int[10000]` | |
| attack_element_level | item_attack_element_levels | `0x6517b9` | `int[10000]` | |
| defense_element | item_defense_elements | `0x653ec9` | `int[10000]` | |
| defense_element_level | item_defense_element_levels | `0x6565d9` | `int[10000]` | |
| owners | item_owners | `0x660219` | `int[10000][100]` | `character` |
| haishera | item_haishera_flags | `0x754459` | `int[10000]` | |
| exclusive_users | item_exclusive_users | `0x756b69` | `int[10000][100]` | `character` |
| female_only | item_female_only_flags | `0x84ada9` | `int[10000]` | |
| required_skill | item_required_skills | `0x859809` | `int[10000]` | `skill` |
| cursed | item_cursed_flags | `0x85bf19` | `int[10000]` | |
| reach | item_reaches | `0x85e629` | `int[10000]` | |
| target_kind | item_target_kinds | `0x860d39` | `int[10000]` | |
| area_width | item_area_widths | `0x863449` | `int[10000]` | |
| area_height | item_area_heights | `0x865b59` | `int[10000]` | |
| charge_time | item_charge_times | `0x868269` | `int[10000]` | |
| min_charge_time | item_min_charge_times | `0x86a979` | `int[10000]` | |
| recovery_time | item_recovery_times | `0x86d089` | `int[10000]` | |
| min_recovery_time | item_min_recovery_times | `0x86f799` | `int[10000]` | |
| condition_levels | item_condition_levels | `0x871ea9` | `int[10000][60]` | |
| equip_condition_levels | item_equip_condition_levels | `0x904669` | `int[10000][60]` | |
| accuracy | item_accuracy_bonuses | `0x996e29` | `int[10000]` | |
| evasion | item_evasion_bonuses | `0x999539` | `int[10000]` | |
| physical_attack | item_physical_attack_bonuses | `0x99bc49` | `int[10000]` | |
| physical_defense | item_physical_defense_bonuses | `0x99e359` | `int[10000]` | |
| attack_count | item_attack_count_bonuses | `0x9a0a69` | `int[10000]` | |
| defense_count | item_defense_count_bonuses | `0x9a3179` | `int[10000]` | |
| magic_attack | item_magic_attack_bonuses | `0x9a5889` | `int[10000]` | |
| magic_defense | item_magic_defense_bonuses | `0x9a7f99` | `int[10000]` | |
| magic_power | item_magic_power_bonuses | `0x9aa6a9` | `int[10000]` | |
| physical_resistance | item_physical_resistance_bonuses | `0x9acdb9` | `int[10000]` | |
| mental_resistance | item_mental_resistance_bonuses | `0x9af4c9` | `int[10000]` | |
| physical_speed | item_physical_speed_bonuses | `0x9b1bd9` | `int[10000]` | |
| mental_speed | item_mental_speed_bonuses | `0x9b42e9` | `int[10000]` | |
| luck | item_luck_bonuses | `0x9b69f9` | `int[10000]` | |
| ct_rate | item_ct_rate_bonuses | `0x9b9109` | `int[10000]` | |
| max_hp | item_max_hp_bonuses | `0x9ca279` | `int[10000]` | |
| max_mp | item_max_mp_bonuses | `0x9cc989` | `int[10000]` | |
| max_tp | item_max_tp_bonuses | `0x9cf099` | `int[10000]` | |
| hp | item_hp_amounts | `0x9bb819` | `int[10000]` | |
| mp | item_mp_amounts | `0x9bdf29` | `int[10000]` | |
| tp | item_tp_amounts | `0x9c0639` | `int[10000]` | |
| fs | item_fs_amounts | `0x9c2d49` | `int[10000]` | |
| control_rate_change | item_control_rate_changes | `0x9c5459` | `int[10000]` | |
| random_amount | item_random_amounts | `0x9d3eb9` | `int[10000]` | |
| animation | item_animations | `0x9d65c9` | `int[10000]` | |
| buy_price | item_buy_prices | `0x9d8cd9` | `int[10000]` | |
| sell_price | item_sell_prices | `0x9db3e9` | `int[10000]` | |
| menu_use | item_menu_use_masks | `0x9ddaf9` | `int[10000]` | |
| granted_skill | item_granted_skills | `0x9e0209` | `int[10000]` | `skill` |
| granted_skill_level | item_granted_skill_levels | `0x9e2919` | `int[10000]` | |
| count | item_counts | `0x10cb17` | `int[10000]` | |
| obtained | item_obtained_flags | `0x10f227` | `int[10000]` | |
| growth_exp | item_growth_exps | `0x111937` | `int[10000]` | |
| growth_level | item_growth_levels | `0x114049` | `int[10000]` | |

### item_growth[10000]

Built by: `boot`.

IMINIT growth paths keyed by item id (128 equipment items); the item's level and exp are item_growth_levels and item_growth_exps.

Record r is record r of `item`.

| field | global | address | type | refers to |
|---|---|---|---|---|
| kind | item_growth_kinds | `0x42edc9` | `int[10000]` | |
| costs | item_growth_costs | `0x4314d9` | `int[10000][11]` | |
| accuracy | item_growth_accuracy_bonuses | `0x44c289` | `int[10000][10]` | |
| evasion | item_growth_evasion_bonuses | `0x464929` | `int[10000][10]` | |
| physical_attack | item_growth_physical_attack_bonuses | `0x47cfc9` | `int[10000][10]` | |
| physical_defense | item_growth_physical_defense_bonuses | `0x495669` | `int[10000][10]` | |
| attack_count | item_growth_attack_count_bonuses | `0x4add09` | `int[10000][10]` | |
| defense_count | item_growth_defense_count_bonuses | `0x4c63a9` | `int[10000][10]` | |
| magic_attack | item_growth_magic_attack_bonuses | `0x4dea49` | `int[10000][10]` | |
| magic_defense | item_growth_magic_defense_bonuses | `0x4f70e9` | `int[10000][10]` | |
| magic_power | item_growth_magic_power_bonuses | `0x50f789` | `int[10000][10]` | |
| physical_speed | item_growth_physical_speed_bonuses | `0x558b69` | `int[10000][10]` | |
| mental_speed | item_growth_mental_speed_bonuses | `0x571209` | `int[10000][10]` | |
| ct_rate | item_growth_ct_rate_bonuses | `0x5a1f49` | `int[10000][10]` | |
| element_level | item_growth_element_level_bonuses | `0x5ba5e9` | `int[10000][10]` | |
| result_item | item_growth_results | `0x5d2c89` | `int[10000]` | `item` |
| flag | item_growth_flags | `0x5d5399` | `int[10000]` | |

### item_class[60]

Built by: `boot`.

AFINIT classes keyed by class id (ITINIT's item_classes): equipment kinds, magic schools and art schools, each with its own rank.

| field | global | address | type |
|---|---|---|---|
| name | class_names | `GS 0xf674` | `string[60]` |
| short_name | class_short_names | `GS 0xf6b0` | `string[60]` |
| equipment_note | class_equipment_notes | `GS 0xf6ec` | `string[60]` |
| icon | class_icons | `0xc0b6fd` | `int[60]` |
| sort_key | class_sort_keys | `0xc0b739` | `int[60]` |
| required_weapons | class_required_weapon_masks | `0xc0b775` | `int[60]` |
| rank_exps | class_rank_exps | `0xc0b7b1` | `int[60][7]` |
| append_number | class_appends | `0xc0b955` | `int[60]` |

### formation[16]

Built by: `boot`.

AFINIT formations keyed by formation id (the party's is G[0x266374 + party]); names and effect text only.

| field | global | address | type |
|---|---|---|---|
| name | formation_names | `GS 0xf728` | `string[16]` |
| effect_lines | formation_effect_lines | `GS 0xf738` | `string[16][14]` |

### school_tree[60]

Built by: `boot`.

SKINIT skill trees keyed by school, an AFINIT item_class (21 to 55): the action in each of 15 slots, 0 none.

Record r is record r of `item_class`.

| field | global | address | type | refers to |
|---|---|---|---|---|
| actions | school_tree_actions | `0x9e5029` | `int[60][15]` | `item` |

### action_tree[10000]

Built by: `boot`.

SKINIT's facts about an action, keyed by its ITINIT id: its tree slot, its prerequisites and the flag it needs.

Record r is record r of `item`.

| field | global | address | type | refers to |
|---|---|---|---|---|
| slot | action_tree_slots | `0x9e53ad` | `int[10000]` | |
| prerequisites | action_prerequisite_actions | `0x9e7abd` | `int[10000][3]` | `item` |
| required_flag | action_required_flags | `0x9eefed` | `int[10000]` | |

### skill[600]

Built by: `boot`.

SSINIT skills keyed by skill id (1 to 524), five levels each. `skill[2].names` are 即死Ⅰ to 即死Ⅴ.

| field | global | address | type | refers to |
|---|---|---|---|---|
| names | skill_names | `GS 0x811c` | `string[600][5]` | |
| descriptions | skill_descriptions | `GS 0x8cd4` | `string[600][35]` | |
| append_number | skill_append_numbers | `0x9f31b9` | `int[600]` | |
| sort_key | skill_sort_keys | `0x9f3411` | `int[600]` | |
| unlock_scenes | skill_unlock_scenes | `0x9f3669` | `int[600][3]` | |
| kind | skill_kinds | `0x9f3d71` | `int[600]` | |
| attack_only | skill_attack_only_flags | `0x9f3fc9` | `int[600]` | |
| effect | skill_effects | `0x9f4221` | `int[600]` | |
| effect_side | skill_effect_sides | `0x9f4479` | `int[600]` | |
| hp_costs | skill_hp_costs | `0x9f46d1` | `int[600][5]` | |
| mp_costs | skill_mp_costs | `0x9f5289` | `int[600][5]` | |
| tp_costs | skill_tp_costs | `0x9f5e41` | `int[600][5]` | |
| trigger_chances | skill_trigger_chances | `0x9f69f9` | `int[600][5]` | |
| amounts | skill_amounts | `0x9f75b1` | `int[600][5]` | |
| support_modes | skill_support_modes | `0x9f8169` | `int[600][5]` | |
| always_on | skill_always_on_flags | `0x9f8d21` | `int[600][5]` | |
| condition_levels | skill_condition_levels | `0x9f98d9` | `int[600][300]` | |
| support_actions | skill_support_actions | `0xa257f9` | `int[600][5]` | `item` |
| support_periods | skill_support_periods | `0xa263b1` | `int[600][5]` | |

### condition[60]

Built by: `boot`.

ILINIT conditions keyed by condition id (1 to 45), five levels each. `condition[7].names` are 呪い1 to 呪い5.

| field | global | address | type |
|---|---|---|---|
| names | condition_names | `GS 0x7d98` | `string[60][5]` |
| resistance | condition_resistances | `0x9f16ff` | `int[60][5]` |
| affinity_columns | condition_affinity_columns | `0x9f182b` | `int[60][5]` |
| durations | condition_durations | `0x9f1957` | `int[60][5]` |
| hit_modifiers | condition_hit_modifiers | `0x9f1a83` | `int[60][5]` |
| evasion_modifiers | condition_evasion_modifiers | `0x9f1baf` | `int[60][5]` |
| physical_attack_modifiers | condition_physical_attack_modifiers | `0x9f1cdb` | `int[60][5]` |
| physical_defense_modifiers | condition_physical_defense_modifiers | `0x9f1e07` | `int[60][5]` |
| attack_count_modifiers | condition_attack_count_modifiers | `0x9f1f33` | `int[60][5]` |
| defense_count_modifiers | condition_defense_count_modifiers | `0x9f205f` | `int[60][5]` |
| magic_attack_modifiers | condition_magic_attack_modifiers | `0x9f218b` | `int[60][5]` |
| magic_defense_modifiers | condition_magic_defense_modifiers | `0x9f22b7` | `int[60][5]` |
| magic_power_modifiers | condition_magic_power_modifiers | `0x9f23e3` | `int[60][5]` |
| physical_resistance_modifiers | condition_physical_resistance_modifiers | `0x9f250f` | `int[60][5]` |
| mental_resistance_modifiers | condition_mental_resistance_modifiers | `0x9f263b` | `int[60][5]` |
| physical_speed_modifiers | condition_physical_speed_modifiers | `0x9f2767` | `int[60][5]` |
| mental_speed_modifiers | condition_mental_speed_modifiers | `0x9f2893` | `int[60][5]` |
| luck_modifiers | condition_luck_modifiers | `0x9f29bf` | `int[60][5]` |
| critical_modifiers | condition_critical_modifiers | `0x9f2aeb` | `int[60][5]` |
| hp_changes | condition_hp_changes | `0x9f2c17` | `int[60][5]` |
| mp_changes | condition_mp_changes | `0x9f2d43` | `int[60][5]` |
| tp_changes | condition_tp_changes | `0x9f2e6f` | `int[60][5]` |
| gauge_drains | condition_gauge_drains | `0x9f2f9b` | `int[60][5]` |
| icon | condition_icons | `0x9f30c7` | `int[60]` |
| polarity | condition_polarities | `0x9f3103` | `int[60]` |
| boss_proof | condition_boss_proof_flags | `0x9f313f` | `int[60]` |
| cure_bits | condition_cure_bits | `0x9f317b` | `int[60]` |

### recipe[1000]

Built by: `boot`.

ALINIT alchemy recipes keyed by recipe id (ids 1 to 810 written); ALCHEMY lists them by output.

| field | global | address | type | refers to |
|---|---|---|---|---|
| output | recipe_outputs | `0x42ab61` | `int[1000]` | `item` |
| party_mask | recipe_party_masks | `0x42af49` | `int[1000]` | |
| required_flags | recipe_required_flags | `0x42b331` | `int[1000][4]` | |
| blocking_flags | recipe_blocking_flags | `0x42c2d1` | `int[1000][2]` | |
| price | recipe_prices | `0x42caa1` | `int[1000]` | |
| ingredients | recipe_ingredients | `0x42ce89` | `int[1000][4]` | `item` |
| ingredient_counts | recipe_ingredient_counts | `0x42de29` | `int[1000][4]` | |

### treasure[10000]

Built by: `boot`.

TRINIT treasures keyed by treasure id (630 written, ids 1 to 5125): five slots of an item and a count, or money. A map object names its treasure; GETTREASURE opens it.

| field | global | address | type | refers to |
|---|---|---|---|---|
| items | treasure_items | `0x6253fc` | `int[10000][5]` | `item` |
| counts | treasure_counts | `0x63174c` | `int[10000][5]` | |
| opened | treasure_opened | `0x109a43` | `int[10000]` | |

### fishing_rod[4]

Built by: `boot`.

EX1INIT rods keyed by k, the item 46 + k.

| field | global | address | type |
|---|---|---|---|
| stats | fishing_rod_stats | `0xc77b38` | `int[4][3]` |
| texts | fishing_rod_texts | `GS 0x10e9a` | `string[4][4]` |

### fishing_bait[8]

Built by: `boot`.

EX1INIT baits keyed by k, the item 601 + k.

| field | global | address | type |
|---|---|---|---|
| hit_rows | fishing_bait_hit_rows | `0xc77b44` | `int[8][5]` |
| texts | fishing_bait_texts | `GS 0x10eaa` | `string[8][4]` |

### fish[15]

Built by: `boot`.

EX1INIT fish keyed by k, the item 611 + k.

| field | global | address | type |
|---|---|---|---|
| stats | fish_stats | `0xc77dc4` | `int[15][4]` |
| element_value | fish_element_values | `0xc77e07` | `int[15]` |
| texts | fish_texts | `GS 0x10ed1` | `string[15][4]` |

### fishing_spot[24]

Built by: `boot`.

EX1INIT fishing places keyed by spot (0 to 8 written); EXTRA1 picks the one whose map is G[0x363f].

| field | global | address | type | refers to |
|---|---|---|---|---|
| fish | fishing_spot_fish | `0xc77e16` | `int[24][15]` | |
| music | fishing_spot_music | `0xc77f7e` | `int[24]` | |
| sound | fishing_spot_sounds | `0xc77f96` | `int[24]` | |
| background | fishing_spot_backgrounds | `0xc77fae` | `int[24]` | pictures |
| overlay | fishing_spot_overlays | `0xc77fc6` | `int[24]` | pictures |
| fisher_pose | fishing_spot_fisher_poses | `0xc77fde` | `int[24]` | |
| cast_ranges | fishing_spot_cast_ranges | `0xc77ff6` | `int[24][12]` | |
| map | fishing_spot_maps | `0xc78116` | `int[24]` | |

### shop_slot[40]

Built by: `shop`.

SHINIT goods of the shop in play, keyed by 20 * page + slot; SHINIT refills them for shop_id each time SHOP opens and after each settlement.

| field | global | address | type |
|---|---|---|---|
| item | shop_items | `0x42ab0f` | `int[40]` |
| stock_limit | shop_stock_limits | `0x42ab37` | `int[40]` |

### scripted_party_slot[7]

Built by: `battle`.

CSINIT party of a scripted battle, keyed by battle slot; CSINIT refills it for scripted_battle_case each time BTL starts one.

| field | global | address | type | refers to |
|---|---|---|---|---|
| character | scripted_party_characters | `0x63dcdb` | `int[7]` | `character` |
| column | scripted_party_columns | `0x63dce2` | `int[7]` | |
| row | scripted_party_rows | `0x63dce9` | `int[7]` | |
| required_flag | scripted_party_required_flags | `0x63dcf0` | `int[7]` | |
| blocking_flag | scripted_party_blocking_flags | `0x63dcf7` | `int[7]` | |

### routine[1000]

Built by: `boot`.

RTINIT battle routines keyed by routine id (EBINIT's G[0x41be47] names a unit's); each field holds 50 steps.

| field | global | address | type |
|---|---|---|---|
| step_kind | routine_step_kinds | `0xa26f6c` | `int[1000][50]` |
| step_chance | routine_step_chances | `0xa332bc` | `int[1000][50]` |
| step_tactic_mask | routine_step_tactic_masks | `0xa3f60c` | `int[1000][50]` |
| step_action | routine_step_actions | `0xa4b95c` | `int[1000][50]` |
| step_arg | routine_step_args | `0xa57cac` | `int[1000][50]` |
| step_percent | routine_step_percents | `0xa63ffc` | `int[1000][50]` |
| step_target_count | routine_step_target_counts | `0xa7034c` | `int[1000][50]` |
| step_flag | routine_step_flags | `0xa889ec` | `int[1000][50]` |
| step_limit | routine_step_limits | `0xa94d3c` | `int[1000][50]` |

### map_routine[1000]

Built by: `boot`.

SRINIT map-unit routines keyed by routine id (the saved map_unit_routines names a unit's); each field holds 20 steps.

| field | global | address | type |
|---|---|---|---|
| step_kind | map_routine_step_kinds | `0xc4bbdb` | `int[1000][20]` |
| step_chance | map_routine_step_chances | `0xc509fb` | `int[1000][20]` |
| step_arg | map_routine_step_args | `0xc5581b` | `int[1000][20]` |
| step_arg_2 | map_routine_step_args_2 | `0xc5a63b` | `int[1000][20]` |
| step_flag | map_routine_step_flags | `0xc6debb` | `int[1000][20]` |
| step_limit | map_routine_step_limits | `0xc72cdb` | `int[1000][20]` |

### battle_animation[10000]

Built by: `boot`.

BTANINIT2 battle animations keyed by animation id (ITINIT's G[0x9d65c9] names an action's).

| field | global | address | type |
|---|---|---|---|
| effects | battle_animation_effects | `0xc25a1a` | `int[10000][6]` |
| effect_starts | battle_animation_effect_starts | `0xc3447a` | `int[10000][6]` |
| length | battle_animation_lengths | `0xc42eda` | `int[10000]` |

### battle_effect[36]

Built by: `battle`.

BTANINIT's effects of the animation in play, keyed by effect slot: 0 to 5 the animation's, 6 to 35 the battlers' conditions (SETEFFECT).

| field | global | address | type | refers to |
|---|---|---|---|---|
| picture | battle_effect_pictures | `0xc455ea` | `int[36]` | pictures |
| still_kind | battle_effect_still_kinds | `0xc4560e` | `int[36]` | |
| blend_mode | battle_effect_blend_modes | `0xc45632` | `int[36]` | |
| width | battle_effect_widths | `0xc45656` | `int[36]` | |
| height | battle_effect_heights | `0xc4567a` | `int[36]` | |
| sheet_columns | battle_effect_sheet_columns | `0xc4569e` | `int[36]` | |
| sheet_rows | battle_effect_sheet_rows | `0xc456c2` | `int[36]` | |
| frame_count | battle_effect_frame_counts | `0xc456e6` | `int[36]` | |
| length | battle_effect_lengths | `0xc4570a` | `int[36]` | |
| placed | battle_effect_placed_flags | `0xc4572e` | `int[36]` | |
| x_offset | battle_effect_x_offsets | `0xc45752` | `int[36]` | |
| y_offset | battle_effect_y_offsets | `0xc45776` | `int[36]` | |
| sound | battle_effect_sounds | `0xc4579a` | `int[36]` | |
| sound_delay | battle_effect_sound_delays | `0xc457be` | `int[36]` | |
| cue_times | battle_effect_cue_times | `0xc457e2` | `int[36][3]` | |

### map_block[750]

Built by: `map`.

RMINIT's blocks of the map in play, keyed by 30 * row + block.

| field | global | address | type | refers to |
|---|---|---|---|---|
| script | map_block_scripts | `0x5d9b73` | `int[750]` | |
| event_script | map_block_event_scripts | `0x5d9e61` | `int[750]` | |
| picture | map_block_pictures | `0x5da14f` | `int[750]` | pictures |
| picture_2 | map_block_pictures_2 | `0x5da43d` | `int[750]` | pictures |
| picture_3 | map_block_pictures_3 | `0x5da72b` | `int[750]` | pictures |
| picture_0 | map_block_pictures_0 | `0x5daff5` | `int[750]` | pictures |
| minimap_picture | map_block_minimap_pictures | `0x5daa19` | `int[750]` | pictures |
| battle_backdrop | map_block_battle_backdrops | `0x5dad07` | `int[750]` | pictures |
| music | map_block_music | `0x5dc189` | `int[750][3]` | |
| encounter_sets | map_block_encounter_sets | `0x5dca53` | `int[750][2]` | |
| enemy_level | map_block_enemy_levels | `0x5dd02f` | `int[750]` | |
| enemy_level_spread | map_block_enemy_level_spreads | `0x5dd31d` | `int[750]` | |
| encounter_level | map_block_encounter_levels | `0x5dd60b` | `int[750]` | |

### encounter_entry[100]

Built by: `encounter`.

ENINIT's entries of the encounter set in play, keyed by entry number.

| field | global | address | type |
|---|---|---|---|
| weight | encounter_entry_weights | `0x63da9d` | `int[100]` |
| gauge_low | encounter_entry_gauge_lows | `0x63db01` | `int[100]` |
| gauge_high | encounter_entry_gauge_highs | `0x63db65` | `int[100]` |
| formations | encounter_entry_formations | `0x63dbc9` | `int[100][2]` |

### formation_slot[8]

Built by: `battle`.

ESINIT's slots of the formation in play, keyed by slot number.

| field | global | address | type |
|---|---|---|---|
| unit | formation_slot_units | `0x63dc92` | `int[8]` |
| first_column | formation_slot_first_columns | `0x63dc9a` | `int[8]` |
| last_column | formation_slot_last_columns | `0x63dca2` | `int[8]` |
| first_row | formation_slot_first_rows | `0x63dcaa` | `int[8]` |
| last_row | formation_slot_last_rows | `0x63dcb2` | `int[8]` |
| chance | formation_slot_chances | `0x63dcba` | `int[8]` |
| required_flag | formation_slot_required_flags | `0x63dcc2` | `int[8]` |
| barring_flag | formation_slot_barring_flags | `0x63dcca` | `int[8]` |
| link | formation_slot_links | `0x63dcd2` | `int[8]` |

### area[100]

Built by: `boot`.

AMINIT2 world-map areas keyed by place id (current_place_id): a dungeon (FIELD) or a town (TOWN).

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | area_names | `GS 0x2244` | `string[100]` | |
| icon | area_icons | `0x42a4c0` | `int[100][2]` | pictures |
| world | area_worlds | `0x42a588` | `int[100]` | |
| kind | area_kinds | `0x42a5ec` | `int[100]` | |
| x | area_x | `0x42a650` | `int[100]` | |
| y | area_y | `0x42a6b4` | `int[100]` | |
| up | area_up_neighbors | `0x42a718` | `int[100]` | `area` |
| down | area_down_neighbors | `0x42a77c` | `int[100]` | `area` |
| left | area_left_neighbors | `0x42a7e0` | `int[100]` | `area` |
| right | area_right_neighbors | `0x42a844` | `int[100]` | `area` |
| cursor_area | area_cursor_areas | `0x42a8a8` | `int[100]` | `area` |

### map[50]

Built by: `boot`.

MPINIT field maps keyed by place id: the start, the encounter numbers and the floors (columns 1 to 20). The sections' scripts and pictures are RMINIT's.

| field | global | address | type | refers to |
|---|---|---|---|---|
| object_sheet | map_object_sheets | `0x5d901f` | `int[50]` | pictures |
| start_floor | map_start_floors | `0x5d9051` | `int[50]` | |
| start_section | map_start_sections | `0x5d9083` | `int[50]` | |
| start_x | map_start_x | `0x5d90b5` | `int[50]` | |
| start_y | map_start_y | `0x5d90e7` | `int[50]` | |
| start_facing | map_start_facings | `0x5d9119` | `int[50]` | |
| encounter_decay_distance | map_encounter_decay_distances | `0x5d914b` | `int[50]` | |
| stray_demon_rate | map_stray_demon_rates | `0x5d917d` | `int[50]` | |
| floor_name_plates | map_floor_name_plates | `0x5d91af` | `int[50][25]` | pictures |
| floor_numbers | map_floor_numbers | `0x5d9691` | `int[50][25]` | |
| floor_names | map_floor_names | `GS 0x2a96` | `string[50][25]` | |

### land[100]

Built by: `boot`.

LAINIT land types keyed by the land index under the player (field_land_id).

| field | global | address | type |
|---|---|---|---|
| footstep_sound | land_footstep_sounds | `0x5f200f` | `int[100]` |
| sprite_column | land_sprite_columns | `0x5f2073` | `int[100]` |
| speed_change | land_speed_changes | `0x5f20d7` | `int[100]` |
| drift_x | land_drift_x | `0x5f213b` | `int[100]` |
| drift_y | land_drift_y | `0x5f219f` | `int[100]` |
| drift_walkable | land_drift_walkable | `0x5f2203` | `int[100]` |
| encounter_class | land_encounter_classes | `0x5f232f` | `int[100]` |

### object_kind[1000]

Built by: `boot`.

OBINIT and OBINIT2 map object kinds, one id in both: the sprite rectangle, the depth, the wall polygon and what the object turns into.

| field | global | address | type |
|---|---|---|---|
| sprite_x | object_kind_sprite_x | `0x5f2393` | `int[1000]` |
| sprite_y | object_kind_sprite_y | `0x5f277b` | `int[1000]` |
| width | object_kind_widths | `0x5f2b63` | `int[1000]` |
| height | object_kind_heights | `0x5f2f4b` | `int[1000]` |
| blend_mode | object_kind_blend_modes | `0x5f3333` | `int[1000]` |
| depth_offset | object_kind_depth_offsets | `0x5f371b` | `int[1000]` |
| wall_point_count | object_kind_wall_point_counts | `0x5f3b03` | `int[1000]` |
| wall_point_x | object_kind_wall_point_x | `0x5f3eeb` | `int[1000][100]` |
| wall_point_y | object_kind_wall_point_y | `0x60c58b` | `int[1000][100]` |
| next_kind | object_kind_next_kinds | `0x624c2b` | `int[1000]` |
| change_sound | object_kind_change_sounds | `0x625013` | `int[1000]` |

### arena_match[100]

Built by: `boot`.

ARINIT arena matches keyed by match id (arena_match_id).

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | arena_match_names | `GS 0x22c6` | `string[100]` | |
| descriptions | arena_match_descriptions | `GS 0x232a` | `string[100][5]` | |
| kind | arena_match_kinds | `0x5d7b04` | `int[100]` | |
| previous | arena_match_previous | `0x5d7b68` | `int[100]` | `arena_match` |
| characters | arena_match_characters | `0x5d7bcc` | `int[100][5]` | |
| fee | arena_match_fees | `0x5d7dc0` | `int[100]` | |
| required_flags | arena_match_required_flags | `0x5d7e24` | `int[100][3]` | |
| closing_flags | arena_match_closing_flags | `0x5d7f50` | `int[100][3]` | |
| battle_mode | arena_match_battle_modes | `0x5d807c` | `int[100]` | |
| enemy_level | arena_match_enemy_levels | `0x5d80e0` | `int[100]` | |
| battles | arena_match_battles | `0x5d8144` | `int[100][2]` | |
| scenes | arena_match_scenes | `0x5d820c` | `int[100][3]` | |
| prize | arena_match_prizes | `0x5d8338` | `int[100]` | |
| reward_items | arena_match_reward_items | `0x5d839c` | `int[100][2]` | `item` |
| reward_counts | arena_match_reward_counts | `0x5d8464` | `int[100][2]` | |

### terrain[50]

Built by: `boot`.

SOINIT war-game terrain, ids 0 to 9.

| field | global | address | type |
|---|---|---|---|
| name | terrain_names | `GS 0x10cb0` | `string[50]` |
| move_costs | terrain_move_costs | `0xc4b25d` | `int[50][4]` |

### troop[30]

Built by: `boot`.

TPINIT war-game troops.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | troop_names | `GS 0x10ce2` | `string[30]` | |
| sheets | troop_sheets | `0xc4b325` | `int[30][2]` | pictures |
| move_type | troop_move_types | `0xc4b361` | `int[30]` | |
| stats | troop_stats | `0xc4b37f` | `int[30][4]` | |
| tactic | troop_tactics | `0xc4b3f7` | `int[30]` | `tactic` |
| soldiers | troop_soldiers | `0xc4b415` | `int[30]` | |
| morale | troop_morale | `0xc4b433` | `int[30]` | |
| rout_points | troop_rout_points | `0xc4b451` | `int[30]` | |

### commander[200]

Built by: `boot`.

CAINIT war-game commanders.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | commander_names | `GS 0x10d00` | `string[200]` | |
| leadership | commander_leaderships | `0xc4b46f` | `int[200]` | |
| attack | commander_attacks | `0xc4b537` | `int[200]` | |
| tactic | commander_tactics | `0xc4b5ff` | `int[200]` | `tactic` |
| duel_battle | commander_duel_battles | `0xc4b6c7` | `int[200]` | |
| sheets | commander_sheets | `0xc4b78f` | `int[200][2]` | pictures |

### tactic[100]

Built by: `boot`.

RUINIT war-game tactics of troops and commanders.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | tactic_names | `GS 0x10dc8` | `string[100]` | |
| description | tactic_descriptions | `GS 0x10e2c` | `string[100]` | |
| kind | tactic_kinds | `0xc4b91f` | `int[100]` | |
| min_range | tactic_min_ranges | `0xc4b983` | `int[100]` | |
| max_range | tactic_max_ranges | `0xc4b9e7` | `int[100]` | |
| power | tactic_powers | `0xc4ba4b` | `int[100]` | |
| cooldown | tactic_cooldowns | `0xc4baaf` | `int[100]` | |
| blend_mode | tactic_blend_modes | `0xc4bb13` | `int[100]` | |
| animation | tactic_animations | `0xc4bb77` | `int[100]` | pictures |

### rank[10]

Built by: `boot`.

GRINIT war-game ranks, 1 to 5.

| field | global | address | type |
|---|---|---|---|
| name | rank_names | `GS 0x10e90` | `string[10]` |
| points | rank_points | `0xc77afb` | `int[10]` |
| bonuses | rank_bonuses | `0xc77b05` | `int[10][4]` |

### town_site[30]

Built by: `town`.

SIINIT's sites of the town in play, keyed by site number (from 1).

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | town_site_names | `GS 0x22a8` | `string[30]` | |
| picture | town_site_pictures | `0x42a90e` | `int[30]` | pictures |
| x | town_site_x | `0x42a92c` | `int[30]` | |
| y | town_site_y | `0x42a94a` | `int[30]` | |
| hot_left | town_site_hot_lefts | `0x42a968` | `int[30]` | |
| hot_top | town_site_hot_tops | `0x42a986` | `int[30]` | |
| hot_width | town_site_hot_widths | `0x42a9a4` | `int[30]` | |
| hot_height | town_site_hot_heights | `0x42a9c2` | `int[30]` | |
| neighbors | town_site_neighbors | `0x42a9e0` | `int[30][4]` | `town_site` |
| state | town_site_states | `0x36b1` | `int[30]` | |

### war_unit[30]

Built by: `stage`.

STINIT's units of the war stage in play, keyed by unit slot 1 to 29. Every array is saved.

| field | global | address | type | refers to |
|---|---|---|---|---|
| side | war_unit_sides | `0x262f36` | `int[30]` | |
| troop | war_unit_troops | `0x262f54` | `int[30]` | `troop` |
| commanders | war_unit_commanders | `0x262f72` | `int[30][4]` | `commander` |
| x | war_unit_x | `0x266312` | `int[30]` | |
| y | war_unit_y | `0x266330` | `int[30]` | |
| facing | war_unit_facings | `0x26634e` | `int[30]` | |
| mark | war_unit_marks | `0x2662d6` | `int[30]` | |
| reserve | war_unit_reserves | `0x2662f4` | `int[30]` | |
| routine | war_unit_routines | `0x2659ea` | `int[30]` | |

### puzzle_block[8]

Built by: `puzzle`.

PZINIT's blocks of the box puzzle in play.

| field | global | address | type |
|---|---|---|---|
| x | puzzle_block_x | `0x63df17` | `int[8]` |
| y | puzzle_block_y | `0x63df1f` | `int[8]` |
| kind | puzzle_block_kinds | `0x63df27` | `int[8]` |
| flag | puzzle_block_flags | `0x63df2f` | `int[8]` |

### scene[10000]

Built by: `boot`.

SCINIT scenes keyed by scene id: SCJUMP names one, SYSTEM4 plays its script.

| field | global | address | type |
|---|---|---|---|
| script | scene_script_ids | `0x4256a0` | `int[10000]` |
| offer_context | scene_offer_contexts | `0x427db0` | `int[10000]` |

### objective[2000]

Built by: `boot`.

FAINIT objectives, the 情報 screen INFO1: listed in sort-key order while their flags allow.

| field | global | address | type |
|---|---|---|---|
| sort_key | objective_sort_keys | `0xc4584e` | `int[2000]` |
| required_flags | objective_required_flags | `0xc4601e` | `int[2000][3]` |
| done_flags | objective_done_flags | `0xc4778e` | `int[2000][3]` |
| chapter | objective_chapters | `GS 0xf860` | `string[2000]` |
| text | objective_texts | `GS 0x10030` | `string[2000]` |

### glossary_entry[1000]

Built by: `boot`.

VIINIT glossary entries, the screen INFO2; the text comes through the VIMES chain.

| field | global | address | type | refers to |
|---|---|---|---|---|
| title | glossary_titles | `GS 0x108c8` | `string[1000]` | |
| unlock_scenes | glossary_unlock_scenes | `0xc49478` | `int[1000][3]` | `scene` (value − 1) |
| picture | glossary_pictures | `0xc4a030` | `int[1000]` | pictures |
| picture_x_offset | glossary_picture_x_offsets | `0xc4a418` | `int[1000]` | |
| picture_y_offset | glossary_picture_y_offsets | `0xc4a800` | `int[1000]` | |

### quest[200]

Built by: `inn`.

QUINIT quests, rebuilt by the INN chain for the current inn: ids 1 to 99 every inn's, 100 to 199 the current inn's own.

| field | global | address | type | refers to |
|---|---|---|---|---|
| name | quest_names | `GS 0x251e` | `string[200]` | |
| text | quest_texts | `GS 0x25e6` | `string[200][6]` | |
| prerequisite | quest_prerequisites | `0x5d852f` | `int[200]` | |
| party | quest_parties | `0x5d85f7` | `int[200]` | |
| required_flags | quest_required_flags | `0x5d86bf` | `int[200][3]` | |
| barring_flags | quest_barring_flags | `0x5d8917` | `int[200][3]` | |
| difficulty | quest_difficulties | `0x5d8b6f` | `int[200]` | |
| money_reward | quest_money_rewards | `0x5d8c37` | `int[200]` | |
| reward_items | quest_reward_items | `0x5d8cff` | `int[200][2]` | `item` |
| reward_counts | quest_reward_counts | `0x5d8e8f` | `int[200][2]` | |

### cg[2000]

Built by: `boot`.

CGINIT gallery CGs keyed by CG id; SETMEMOIR places each by tab, slot and variant.

| field | global | address | type | refers to |
|---|---|---|---|---|
| picture | cg_pictures | `0x269b32` | `int[2000][2]` | pictures |
| tab | cg_tabs | `0x26aad2` | `int[2000]` | |
| slot | cg_slots | `0x26b2a2` | `int[2000]` | |
| variant | cg_variants | `0x26ba72` | `int[2000]` | |
| scroll_x | cg_scroll_x | `0x26c242` | `int[2000]` | |
| scroll_y | cg_scroll_y | `0x26ca12` | `int[2000]` | |
| append_number | cg_append_numbers | `0x26d1e2` | `int[2000]` | |

### replay[150]

Built by: `boot`.

SPINIT scene replays keyed by page * 15 + cell (10 pages of 15; HMODE shows 5, or 6 with an append).

| field | global | address | type |
|---|---|---|---|
| script | replay_scripts | `0x27513d` | `int[150]` |
| append_number | replay_append_numbers | `0x2751d3` | `int[150]` |

### music_track[50]

Built by: `boot`.

MUINIT music room tracks keyed by their place in the room.

| field | global | address | type |
|---|---|---|---|
| bgm_number | music_track_bgm_numbers | `0x275334` | `int[50]` |
| unlock_resource | music_track_unlock_resources | `0x275302` | `int[50]` |

### Scope dungeon

Ids 1 to 49. A data file reaches each table below as `dungeon.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A dungeon map, by current_place_id (an MPINIT id; the game's are 1 to 47, and its per-map arrays hold 50). RMINIT clears these tables and fills them from the branch for the map in play, each layer's in turn, each time FIELD starts and when a save made on a map is loaded.

| name | table |
|---|---|
| block | map_block |

### Scope war_stage

Ids 1 to 5. A data file reaches each table below as `war_stage.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A war stage, by war_stage_id. STINIT fills these tables from the branch for the stage in play.

| name | table |
|---|---|
| unit | war_unit |

### Scope town

Ids 50 to 63. A data file reaches each table below as `town.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A town, by current_place_id. SIINIT clears these tables and fills them from the branch for the town in play.

| name | table |
|---|---|
| site | town_site |

### Scope puzzle

Ids 1 to 14. A data file reaches each table below as `puzzle.<id>.<name>.<record>` and not by its own name (data patches spec §6.2).

A box puzzle, by field_puzzle_id. PZINIT clears these tables and fills them from the branch for the puzzle in play.

| name | table |
|---|---|
| block | puzzle_block |

## choice-output

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x309d` | scjump_chain_pending | high | investigation | A walker of scjump_chain_script_ids sets 1 and walks while it is 1; the first rule that holds sets current_scene_id (or, in contexts 3 and 8, records its scene) and clears it. |
| `0x269b30` | current_scene_id | high | investigation | The scene to play, an id of SCINIT. A SCJUMP rule sets it; SYSTEM4 plays it and a scene script branches on it. Above the saved prefix. |
| `0x42aa58` | town_location_scenes | high | investigation | The scene a context 3 rule gives each town location. TOWN@0x1ca clears it, and TOWN@0x6d4 plays a location's scene when the player enters it. |
| `0x42aa76` | town_location_scene_icons | low | investigation | Five values a context 3 rule gives with a location's scene. TOWN@0x1ca clears them and TOWN@0x8ab draws an icon for each nonzero one (value - 1); they look like characters, not confirmed. |
| `0x5d852e` | quest_contract_held | high | investigation | CHECKCONTRACT's answer: 1 when the party in play holds current_quest_id (CHECKCONTRACT@0x5c). |
| `0xc7812f` | chain_handled | high | investigation | The DRAW*MES, INFO2, INFO3 and SETMAPDATA chain walkers clear it and stop once a chain script sets it to 1, which a script does when it has handled the id (VIMES@0x1726). |

## counter

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x3626` | scene_queue_count | high | investigation | How many scenes scene_queue holds. A rule that takes the turn clears it; context 8's rules add one. |
| `0x3627` | scene_queue_index | med | investigation | The scene_queue entry SYSTEM4@0x31f plays next. |
| `0x36e1` | field_gauge | med | investigation | Saved: a gauge of -15 to 15 that FIELD lowers as the party walks (FIELD@0x1591) and USESKILL changes (USESKILL@0x15); ENINIT entries are drawn by it (SETENP@0x2e3). Its name in the game is not read. |
| `0x36e3` | field_distance | med | investigation | Saved: the distance walked, which FIELD adds to (FIELD@0x15a4); each 20 is an encounter roll, and the distance band is EEINIT's row (FIELD@0x19a7). |
| `0x10cb17` | item_counts | high | investigation | Saved: how many of each item the player holds, 0 to 999 (ADDITEM@0x8..0x5d). GAMECLEAR@0x3e1 stores it in the shared profile for classes over 1. |
| `0x111937` | item_growth_exps | high | investigation | Saved: an equipment's growth exp; BTL@0x6017 adds 1 a battle for a growth kind 2 item, RESULT@0xab2 empties it at a level up. |
| `0x114049` | item_growth_levels | high | investigation | Saved: an equipment's growth level; SETCH@0x754 adds the IMINIT bonuses of every level below it. |
| `0x1196e2` | character_levels | high | investigation | Saved: the character's level. UNITECH@0x0020d sets it to unit_base_levels; CALCABIMAX sets a lead's from its parameters. |
| `0x119746` | character_exp | med | investigation | Saved: the character's experience; AUTOGROW spends it on a lead's parameters. |
| `0x119eb2` | character_class_ranks | high | investigation | Saved: a character's rank 1..7 (E..M) in each class; CHMODE1@0x1caf refuses an item of a higher rank, BTL@0x48d2 raises it. |
| `0x11b622` | character_class_exps | high | investigation | Saved: a character's exp in each class; BTL@0x48ae adds 5 × G[0x1196e2 + char] plus a random 0..99 a battle. |
| `0x25fa0b` | shop_bought_counts | high | investigation | Saved: how many of a shop slot's item have been bought, at 40 * shop + 20 * page + slot; SHOP adds each purchase (SHOP@0x01487) and subtracts it from the slot's stock limit. |
| `0x265a08` | map_unit_routine_step_uses | high | investigation | Saved: how many times each map unit has run each step of its routine (RTN_M001@0x0), against map_routine_step_limits (MVRTN@0x14c). |
| `0x26636c` | fishing_points | high | investigation | Saved: the fishing points, at most 99999; EXTRA1 loads them on entry and stores them on leaving (EXTRA1@0x0033a, 0x07c39). |
| `0x266377` | shop_money_spent | med | investigation | Saved: the money a party has spent in shops; SHOP adds each bill (SHOP@0x0154e). |
| `0x26637a` | alchemy_money_spent | med | investigation | Saved: the money a party has spent on alchemy; ALCHEMY adds the bill for parties 0 and 1 (ALCHEMY@0x01c5c) and tests the total against 10000 (ALCHEMY@0x03f24). |
| `0x26637e` | war_points | med | investigation | Saved: the points GRINIT's ranks are reached by (SLG@0x231). |
| `0x26d9b2` | cg_unlocked_percent | high | investigation | SETMEMOIR@0x20a: the share of the gallery's CGs unlocked, in percent. |
| `0x274ee5` | cg_slot_unlocked_counts | high | investigation | SETMEMOIR: how many of a slot's CGs are unlocked. |
| `0x275011` | cg_slot_counts | high | investigation | SETMEMOIR: how many CGs a slot holds; CGMODE@0x1531 reads it. |
| `0x275269` | replay_unlocked_percent | high | investigation | SETMEMOIR@0x33f: the share of the replays unlocked, in percent. |
| `0x275366` | music_unlocked_percent | high | investigation | SETMEMOIR@0x3c3: the share of the tracks unlocked, in percent; MMODE@0x5f4 draws it. |
| `0xc1c436` | battler_routine_step_uses | high | investigation | How many times each battler has run each step of its routine in this battle (RTN@0x1c3, RTN@0x25e), against routine_step_limits. |

## data-table

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x355e` | scene_queue | high | investigation | Scenes SCJUMP's context 8 rules queue, with their quests. SYSTEM4@0x31f plays them in turn and, after one with a quest, sets current_quest_id and runs QUESTCLEAR (SYSTEM4@0x631). Columns: 0=scene, 1=quest. |
| `0x3640` | area_states | high | investigation | Written by AMINIT each time the world map opens: bit 0 shows the area (ALLMAP@0xed6), bit 1 lets the player enter it (ALLMAP@0xdf3). In the saved prefix, rebuilt on every opening. |
| `0x36a8` | world_map_music | high | investigation | AMINIT: the world map's music (0x2b, 0x31). |
| `0x36a9` | world_map_pictures | high | investigation | AMINIT: the world map's six pictures (AL001.AGF ...). |
| `0x36b1` | town_site_states | high | investigation | Written by SIINIT on every town visit: bit 0 the site is open (TOWN@0x1f6), and TOWN@0x20b runs SCJUMP's context 3 for such a site; a context 3 rule sets bit 1 when it gives the site a scene. In the saved prefix, rebuilt on every visit. |
| `0x109a43` | treasure_opened | med | investigation | Saved: 1 once a treasure has been opened, keyed by treasure id; GETTREASURE gives nothing for an opened one and sets it before calling GETITEM (GETTREASURE@0x00048). |
| `0x10c4d7` | puzzle_saved_block_x | med | investigation | The saved column of each block of each puzzle, set from PZINIT on the puzzle's first entry (SETMAPDATA@0x174..0x1d4) and played from by CARRY. |
| `0x10c7f7` | puzzle_saved_block_y | med | investigation | The saved row of each block, set with puzzle_saved_block_x. |
| `0x116759` | arena_match_wins | high | investigation | Saved: 1 once the match is won; RESULT@0x395 sets it and pays the prize only then, ARENA@0xb1f tests the previous match's. |
| `0x119106` | character_parameters | high | investigation | Saved: a lead's seven growth parameters, copied from lead_starting_parameters on joining (UNITECH@0x001ed) and raised by AUTOGROW. |
| `0x1193c2` | character_parameter_caps | high | investigation | Saved: each parameter's cap, which CALCABIMAX computes from the CBINIT row. |
| `0x16affb` | title_earned_flags | high | investigation | Saved: 1 once the title is earned (ADDTITLE@0x0014c); GAMECLEAR@0x003b4 stores it to the shared profile and GAMESTART@0x00e45 loads it. |
| `0x25febb` | scene_seen_flags | high | investigation | SYSTEM4@0x3b0 sets [current_scene_id] to 1 when a scene plays and stores the cell in the shared profile. INFO2 unlocks a glossary entry from it. |
| `0x262e7e` | war_stage_terrain | high | investigation | STINIT: the battlefield's terrain, SOINIT ids, 10 rows of 18; saved. |
| `0x262f34` | war_stage_picture | high | investigation | STINIT: the battlefield's picture (ST001.AGF ...); saved. |
| `0x262f36` | war_unit_sides | high | investigation | STINIT: the unit's side, 1 or 2; saved. |
| `0x262f54` | war_unit_troops | high | investigation | STINIT: the unit's troop, a TPINIT id (SLG@0x3ba); saved. |
| `0x262f72` | war_unit_commanders | high | investigation | STINIT: up to four commanders, CAINIT ids; saved. |
| `0x2659ea` | war_unit_routines | high | investigation | STINIT: the enemy unit's routine; MVRTN@0x87 reads G[0xc4bbdb + 20 * routine + step]. |
| `0x2662d6` | war_unit_marks | low | investigation | STINIT: a mark on two or three units a stage; DRAWUN@0x17c draws an emblem on the unit and SLG@0x1d72 counts it when checking whether a side is beaten. |
| `0x2662f4` | war_unit_reserves | med | investigation | STINIT: 1 keeps the unit off the field at the start (SLG@0x32a); SETRELIEF brings units in later. |
| `0x266312` | war_unit_x | high | investigation | STINIT: the unit's start column (SLG@0x338); saved. |
| `0x266330` | war_unit_y | high | investigation | STINIT: the unit's start row (SLG@0x34d); saved. |
| `0x26634e` | war_unit_facings | high | investigation | STINIT: the unit's facing (SLG@0x3ae, SETRELIEF@0xfa); saved. |
| `0x26637d` | war_stage_gauge_start | high | investigation | STINIT: the war gauge's start; SLG@0x2fa copies it into G[0x266380], bounded at twice it (SLG@0x1ae1). |
| `0x268b40` | scene_graphic_element_numbers | med | investigation | INIT2 constants 10000, 10010, 10110, 10120, 10130, 10140, 10150, 10160, 10210, 10220: graphic element numbers that scenes transform ($1$SC0630@0x936). |
| `0x268b8c` | speaker_voice_units | high | investigation | CNINIT: the unit whose voice setting the speaker uses; the scenes mute the voice when G[0x30a0 + unit_voice_settings[this]] is set (SC0000@0x00bd1). |
| `0x268f74` | unit_voice_settings | high | investigation | CVINIT, by unit: the unit's voice setting; the scenes' mute test (SC0000@0x00bd8). |
| `0x26935c` | voice_setting_units | high | investigation | CVINIT: the setting's unit; CONFIG@0x017bc shows the setting's name once G[0x2625cb] has that unit. |
| `0x269744` | voice_setting_samples | high | investigation | CVINIT: the sample voice clip the 設定 voice page plays (CONFIG@0x01618); the table's leader. |
| `0x269b32` | cg_pictures | med | investigation | CGINIT's leader. 0 the CG (CGMODE@0xb74); 1, on 546, a 204 x 115 picture (EVM...) that SAVE@0xdb5 and ALLMAP@0x3ff draw as a save slot's thumbnail when the screen shows picture 0. Columns: 0=picture, 1=save_thumbnail. |
| `0x26aad2` | cg_tabs | high | investigation | CGINIT: the gallery tab, 1 to 7 (SETMEMOIR@0x5e). |
| `0x26b2a2` | cg_slots | high | investigation | CGINIT: the slot on the tab's page, 1 to 30, six columns of five rows (SETMEMOIR@0x6c, CGMODE@0x14e8). |
| `0x26ba72` | cg_variants | high | investigation | CGINIT: the CG's place within its slot, 1 to 18; 0 means not in the gallery (SETMEMOIR@0x50, 0x7a). |
| `0x26c242` | cg_scroll_x | med | investigation | CGINIT writes none. CGMODE@0xbc1 negates it for the x start of a picture larger than the screen, beside cg_scroll_y. |
| `0x26ca12` | cg_scroll_y | med | investigation | CGINIT, on 3 CGs (261, 432): CGMODE@0xbd4 negates it for the y start of a picture larger than the screen. |
| `0x26d1e2` | cg_append_numbers | high | investigation | CGINIT, on the appends' 99 CGs, written ahead of the record's leader: the append that adds the CG. CGMODE@0x14da draws its thumbnail from texture 0x5e + it (cg_thumbnail_sheet_chain). |
| `0x26d9b5` | cg_gallery_pages | high | investigation | SETMEMOIR (run by ROOM): [tab - 1][100 x (slot - 1) + variant - 1] is the CG's id where its picture is unlocked and -1 where not; CGMODE reads it. |
| `0x27513d` | replay_scripts | high | investigation | SPINIT, by page x 15 + cell: the scene to replay, a packed script id (SP2712.BIN ...; page 5 holds the appends'). HMODE@0xbc1 runs it. |
| `0x2751d3` | replay_append_numbers | high | investigation | SPINIT, page 5 only: the append that adds the scene. HMODE@0x1020 draws its thumbnail from texture 0x5e + it (replay_thumbnail_sheet_chain). |
| `0x27526c` | replay_cell_states | high | investigation | SETMEMOIR: cell + 1 where the replay's script is unlocked and -1 where not (SETMEMOIR@0x266 to 0x2bd); HMODE@0xbaf and 0xfee read it. |
| `0x275302` | music_track_unlock_resources | high | investigation | MUINIT: the catalog resource that unlocks the track, its own file (BGM041.OGG) or, for the theme songs, the script that plays them (OP.BIN, ED2.BIN, ED.BIN). SETMEMOIR@0x393. |
| `0x275334` | music_track_bgm_numbers | high | investigation | MUINIT's leader: the BGM number the music room plays (MMODE@0x511 play-bgm; 41 plays BGM041.OGG). |
| `0x275369` | music_track_unlocked | high | investigation | SETMEMOIR@0x3ad sets 1 where the track's resource is unlocked; MMODE@0x4e7 plays only such a track. |
| `0x36fa06` | lead_starting_parameters | high | investigation | CBINIT: a lead's seven starting parameters. UNITECH@0x001e2 copies them into the saved G[0x119106 + 7 x character] when the lead joins; CALCCHP derives the stats from them. |
| `0x36fcc2` | lead_parameter_cap_level_divisors | high | investigation | CBINIT: a parameter's cap is (level x 1000 / this + sum x 1000 / the sum divisor) / 100 + the addend, at most 999 (CALCABIMAX@0x000bc). |
| `0x36ff7e` | lead_parameter_cap_sum_divisors | high | investigation | CBINIT: the sum divisor of a parameter's cap (CALCABIMAX@0x000ce). |
| `0x37023a` | lead_parameter_cap_addends | high | investigation | CBINIT: the addend of a parameter's cap (CALCABIMAX@0x00120). |
| `0x3704f6` | lead_parameter_costs | high | investigation | CBINIT: the experience a parameter point costs, in percent of the parameter (AUTOGROW@0x00041; CHMODE2@0x01d5b shows it). |
| `0x3707b2` | lead_pool_rates | high | investigation | CBINIT: a percentage on a lead's max HP, MP and TP (CALCCHP@0x000be). |
| `0x370817` | unit_voice_clips | med | investigation | EBINIT: a unit's 30 voice clips. BTL@0x08b2b plays column 27 when the battle starts and column 18 when the unit falls; a slot with a title plays title_voice_clips instead. Columns: 18=fall, 27=battle_start. |
| `0x377d47` | unit_battle_sprites | high | investigation | EBINIT: the unit's battle sprite (a CPB picture). BTL@0x09102 loads it into surface 0x34 + slot; the bestiary draws it (INFO4@0x00cef). |
| `0x37812f` | unit_book_pictures | high | investigation | EBINIT: the bestiary's picture of the unit where it is not the battle sprite (an EI picture, INFO4@0x00ca6). |
| `0x378517` | unit_book_sort_keys | high | investigation | EBINIT: the unit's place in the bestiary. INFO4@0x00784 lists only units with a value above 0, in this order. |
| `0x3788ff` | unit_sexes | high | investigation | EBINIT: the unit's sex. ALCHEMY@0x03d63, CHMODE1@0x01ae0 and SHOP@0x04154 refuse an item for women unless this is 2; BTL@0x02925 tests it for the 性魔術 capture. Values: 1 Male, 2 Female. |
| `0x378ce7` | unit_fs_temperaments | med | investigation | EBINIT: how the slot's FS moves, 1 to 5 (every record writes 3). BTL@0x0438f moves G[0xc1a960] by it; CALCDAMAGE@0x00a29 sets G[0x276e95] from it. |
| `0x3794b7` | unit_overlay_kinds | low | investigation | EBINIT: 2 to 4 for 31 units. BTL@0x09719 draws an extra overlay (0x2717) for slot 6 when it is 3; its meaning is not read. |
| `0x37989f` | unit_boss_classes | med | investigation | EBINIT: the unit's boss class, 1 to 6. BTL@0x001e1 picks the battle music from the enemies' highest class; CALCILL@0x005c1 makes the unit immune to some conditions by it. |
| `0x379c87` | unit_widths | high | investigation | EBINIT: the unit's width on the battle grid, in cells (BTL@0x027bc, SELTARGET@0x00ae1, ANIME@0x0046f). |
| `0x37a06f` | unit_heights | high | investigation | EBINIT: the unit's height on the battle grid, in cells (BTL@0x058c8, SELTARGET@0x00af4, ANIME@0x004a7). |
| `0x37a457` | unit_defense_elements | high | investigation | EBINIT: the unit's defense element, named by AFINIT's GS[0xf660 + value]. SETEN@0x00c02 and SETCH@0x00a89 copy it into the slot's G[0xc1ac8a] when the armor gives none; INFO4@0x01084 draws it. Names: `defense_element_names`, element 0 naming 0. |
| `0x37a83f` | unit_defense_element_levels | high | investigation | EBINIT: the defense element's level, 1 to 5, into the slot's G[0xc1aca8] (SETEN@0x00c15, SETCH@0x00a9c). |
| `0x37ac27` | unit_initial_weapons | high | investigation | EBINIT: the unit's weapon, an ITINIT id. SETEN@0x007f0 equips an enemy slot with it; UNITECH@0x00072 copies it into the saved 主武器 G[0x11cd92 + character] when the character joins. |
| `0x37b00f` | unit_initial_shields | med | investigation | EBINIT: the unit's shield, an ITINIT id (SETEN@0x00808; UNITECH@0x00085 into the saved G[0x11cebe]). No record writes it. |
| `0x37b3f7` | unit_initial_armors | high | investigation | EBINIT: the unit's armor, an ITINIT id (SETEN@0x007fc; UNITECH@0x00098 into the saved 防具 G[0x11cf22]). |
| `0x37b7df` | unit_initial_accessories | high | investigation | EBINIT: the unit's accessory, an ITINIT id (SETEN@0x00814; UNITECH@0x000ab into the saved 装飾 G[0x11cf86]). |
| `0x37bbc7` | unit_equip_kind_flags | high | investigation | EBINIT: the item kinds the unit may equip, bit k for ITINIT kind k. DRAWCHP@0x00eae and CHANGETITLE@0x00133 add the title's kinds and kind 22; SHOP@0x040fa and ALCHEMY@0x03d09 test it. |
| `0x3805ff` | unit_skill_levels | high | investigation | EBINIT: the unit's skill levels, column = an SSINIT skill id. SETEN@0x00cf9 adds the title's and clamps the sum into the slot's G[0xc1ca4e]; the bestiary lists them (INFO4@0x01136). Column k is record k of `skill`. |
| `0x412dbf` | unit_lead_growth_rows | high | investigation | EBINIT: the unit's CBINIT row, set for units 1 and 31 only. Where set, UNITECH@0x001b0 copies the row into the saved parameters and runs CALCCHP instead of CALCLVUP. |
| `0x4131a7` | unit_base_levels | high | investigation | EBINIT: the level the unit's stats are given at. SETEN@0x00184 floors an enemy's level at it; UNITECH@0x0020d sets a joining character's saved level G[0x1196e2] to it. |
| `0x41358f` | unit_max_levels | high | investigation | EBINIT: the highest level of an enemy slot (SETEN@0x0007a); 500 when G[0x266409] is set. |
| `0x413977` | unit_level_catchup_divisors | med | investigation | EBINIT: SETEN@0x000ce raises an enemy's level by (the party's average level - level) / this. Every record writes 1. |
| `0x413d5f` | unit_accuracies | high | investigation | EBINIT: 命中, into the slot's G[0xc1acc6] (SETEN@0x00445; SETCH@0x00196 reads it for a character too). |
| `0x414147` | unit_evasions | high | investigation | EBINIT: 回避, into the slot's G[0xc1ace4] (SETEN@0x00458, SETCH@0x001a9). |
| `0x41452f` | unit_attacks | high | investigation | EBINIT: 攻撃力 at the unit's base level, in hundredths. SETEN@0x0027e puts (this + growth x levels above base) / 100 into the slot's G[0xc1aa50]; CALCLVUP@0x000ce writes this + growth x levels into the character's saved G[0x119a66]. Shown as the value × 0.01. |
| `0x414917` | unit_defenses | high | investigation | EBINIT: 防御力 at the unit's base level, in hundredths. SETEN@0x002a8 puts (this + growth x levels above base) / 100 into the slot's G[0xc1aa6e]; CALCLVUP@0x000ff writes this + growth x levels into the character's saved G[0x119aca]. Shown as the value × 0.01. |
| `0x414cff` | unit_magic_attacks | high | investigation | EBINIT: 魔法攻撃 at the unit's base level, in hundredths. SETEN@0x00326 puts (this + growth x levels above base) / 100 into the slot's G[0xc1aa8c]; CALCLVUP@0x00130 writes this + growth x levels into the character's saved G[0x119b2e]. Shown as the value × 0.01. |
| `0x4150e7` | unit_magic_defenses | high | investigation | EBINIT: 魔法防御 at the unit's base level, in hundredths. SETEN@0x00350 puts (this + growth x levels above base) / 100 into the slot's G[0xc1aaaa]; CALCLVUP@0x00161 writes this + growth x levels into the character's saved G[0x119b92]. Shown as the value × 0.01. |
| `0x4154cf` | unit_attack_counts | high | investigation | EBINIT: 攻撃回数 at the unit's base level, in hundredths. SETEN@0x002d2 puts (this + growth x levels above base) / 100 into the slot's G[0xc1aac8]; CALCLVUP@0x00192 writes this + growth x levels into the character's saved G[0x119bf6]. Shown as the value × 0.01. |
| `0x4158b7` | unit_defense_counts | high | investigation | EBINIT: 防御回数 at the unit's base level, in hundredths. SETEN@0x002fc puts (this + growth x levels above base) / 100 into the slot's G[0xc1aae6]; CALCLVUP@0x001c3 writes this + growth x levels into the character's saved G[0x119c5a]. Shown as the value × 0.01. |
| `0x415c9f` | unit_physical_resistances | high | investigation | EBINIT: 肉体抵抗 at the unit's base level, in hundredths. SETEN@0x0037a puts (this + growth x levels above base) / 100 into the slot's G[0xc1ab04]; CALCLVUP@0x001f4 writes this + growth x levels into the character's saved G[0x119cbe]. Shown as the value × 0.01. |
| `0x416087` | unit_mental_resistances | high | investigation | EBINIT: 精神抵抗 at the unit's base level, in hundredths. SETEN@0x003a4 puts (this + growth x levels above base) / 100 into the slot's G[0xc1ab22]; CALCLVUP@0x00225 writes this + growth x levels into the character's saved G[0x119d22]. Shown as the value × 0.01. |
| `0x41646f` | unit_physical_speeds | high | investigation | EBINIT: 肉体戦速 at the unit's base level, in hundredths. SETEN@0x003ce puts (this + growth x levels above base) / 100 into the slot's G[0xc1ab40]; CALCLVUP@0x00256 writes this + growth x levels into the character's saved G[0x119d86]. Shown as the value × 0.01. |
| `0x416857` | unit_mental_speeds | high | investigation | EBINIT: 精神戦速 at the unit's base level, in hundredths. SETEN@0x003f8 puts (this + growth x levels above base) / 100 into the slot's G[0xc1ab5e]; CALCLVUP@0x00287 writes this + growth x levels into the character's saved G[0x119dea]. Shown as the value × 0.01. |
| `0x416c3f` | unit_luck | high | investigation | EBINIT: 運 at the unit's base level, in hundredths. SETEN@0x00422 puts (this + growth x levels above base) / 100 into the slot's G[0xc1ab7c]; CALCLVUP@0x002b8 writes this + growth x levels into the character's saved G[0x119e4e]. Shown as the value × 0.01. |
| `0x417027` | unit_max_hp_growth | high | investigation | EBINIT: max HP gained a level, in hundredths (SETEN@0x001f2, CALCLVUP@0x00049). Shown as the value × 0.01. |
| `0x41740f` | unit_max_mp_growth | high | investigation | EBINIT: max MP gained a level, in hundredths (SETEN@0x0021c, CALCLVUP@0x0007a). Shown as the value × 0.01. |
| `0x4177f7` | unit_max_tp_growth | high | investigation | EBINIT: max TP gained a level, in hundredths (SETEN@0x00246, CALCLVUP@0x000ab). Shown as the value × 0.01. |
| `0x417bdf` | unit_attack_growth | high | investigation | EBINIT: 攻撃力 gained a level, in hundredths (SETEN@0x00270, CALCLVUP@0x000dc). Shown as the value × 0.01. |
| `0x417fc7` | unit_defense_growth | high | investigation | EBINIT: 防御力 gained a level, in hundredths (SETEN@0x0029a, CALCLVUP@0x0010d). Shown as the value × 0.01. |
| `0x4183af` | unit_magic_attack_growth | high | investigation | EBINIT: 魔法攻撃 gained a level, in hundredths (SETEN@0x00318, CALCLVUP@0x0013e). Shown as the value × 0.01. |
| `0x418797` | unit_magic_defense_growth | high | investigation | EBINIT: 魔法防御 gained a level, in hundredths (SETEN@0x00342, CALCLVUP@0x0016f). Shown as the value × 0.01. |
| `0x418b7f` | unit_attack_count_growth | high | investigation | EBINIT: 攻撃回数 gained a level, in hundredths (SETEN@0x002c4, CALCLVUP@0x001a0). Shown as the value × 0.01. |
| `0x418f67` | unit_defense_count_growth | high | investigation | EBINIT: 防御回数 gained a level, in hundredths (SETEN@0x002ee, CALCLVUP@0x001d1). Shown as the value × 0.01. |
| `0x41934f` | unit_physical_resistance_growth | high | investigation | EBINIT: 肉体抵抗 gained a level, in hundredths (SETEN@0x0036c, CALCLVUP@0x00202). Shown as the value × 0.01. |
| `0x419737` | unit_mental_resistance_growth | high | investigation | EBINIT: 精神抵抗 gained a level, in hundredths (SETEN@0x00396, CALCLVUP@0x00233). Shown as the value × 0.01. |
| `0x419b1f` | unit_physical_speed_growth | high | investigation | EBINIT: 肉体戦速 gained a level, in hundredths (SETEN@0x003c0, CALCLVUP@0x00264). Shown as the value × 0.01. |
| `0x419f07` | unit_mental_speed_growth | high | investigation | EBINIT: 精神戦速 gained a level, in hundredths (SETEN@0x003ea, CALCLVUP@0x00295). Shown as the value × 0.01. |
| `0x41a2ef` | unit_luck_growth | high | investigation | EBINIT: 運 gained a level, in hundredths (SETEN@0x00414, CALCLVUP@0x002c6). Shown as the value × 0.01. |
| `0x41a6d7` | unit_max_hp | high | investigation | EBINIT: max HP at the unit's base level, in hundredths. SETEN@0x00200 puts (this + growth x levels above base) / 100 into the slot's G[0xc1a9ba]; CALCLVUP@0x0003b writes this + growth x levels into the character's saved G[0x119872]. Shown as the value × 0.01. |
| `0x41aabf` | unit_max_mp | high | investigation | EBINIT: max MP at the unit's base level, in hundredths. SETEN@0x0022a puts (this + growth x levels above base) / 100 into the slot's G[0xc1a9f6]; CALCLVUP@0x0006c writes this + growth x levels into the character's saved G[0x11993a]. Shown as the value × 0.01. |
| `0x41aea7` | unit_max_tp | high | investigation | EBINIT: max TP at the unit's base level, in hundredths. SETEN@0x00254 puts (this + growth x levels above base) / 100 into the slot's G[0xc1aa32]; CALCLVUP@0x0009d writes this + growth x levels into the character's saved G[0x119a02]. Shown as the value × 0.01. |
| `0x41b28f` | unit_starting_fs | high | investigation | EBINIT: an enemy slot's FS at the start of battle (SETEN@0x01069); the bestiary draws it (INFO4@0x00e57). |
| `0x41b677` | unit_max_fs | high | investigation | EBINIT: the most FS the unit can hold, into the slot's G[0xc1a97e] (SETEN@0x001e6, SETCH@0x0005d). |
| `0x41ba5f` | unit_first_waits | med | investigation | EBINIT: the wait before the unit's first action, into the slot's G[0xc1c346] and G[0xc1c328] (BTL@0x03a33, SETENP@0x00254, REVIVE@0x00062). |
| `0x41be47` | unit_routine_ids | high | investigation | EBINIT: the unit's AI routine, an RTINIT id. BTL@0x031e2 sets G[0xa26f69] to it and runs RTN. |
| `0x41c22f` | unit_action_ids | high | investigation | EBINIT: the six actions the AI may use, ITINIT ids; 0 means the weapon. RTN@0x0009d copies them to G[0x36fa00]. |
| `0x41d99f` | unit_exp_curves | med | investigation | EBINIT: the experience curve, 1 to 5, of a party unit: CALCNEXTEXP@0x0002b multiplies the level by 200, 215, 225, 235 or 250 x 124 / 1000. |
| `0x41dd87` | unit_exp_rates | med | investigation | EBINIT: a percentage on the experience a party unit's level needs (CALCNEXTEXP@0x000d9). |
| `0x41e16f` | unit_exp_rewards | med | investigation | EBINIT: the experience the battle's pool G[0x276e9a] gets from the unit (BTL@0x05fb4); by our reading column 0 when a hit defeats it, column 1 when it survives. Columns: 0=defeated, 1=survived. |
| `0x41e93f` | unit_money | high | investigation | EBINIT: money. RESULT@0x00542 adds it for each enemy defeated; UNITECH@0x000b7 adds it to the party's saved money G[0x118f0d + party] when the unit's character joins. |
| `0x41ed27` | unit_drop_items | high | investigation | EBINIT: five drop items, ITINIT ids (RESULT@0x00577); UNITECH@0x00137 gives a joining character's to the party through ADDITEM. |
| `0x4200af` | unit_drop_chances | high | investigation | EBINIT: each drop's chance in percent (RESULT@0x006bc). |
| `0x421437` | unit_drop_required_flags | med | investigation | EBINIT: two story flags each drop needs, column 2 x drop + k; flag value - 1 of G[0x929] must be set (RESULT@0x005b9). |
| `0x423b47` | unit_drop_excluding_flags | med | investigation | EBINIT: a story flag that stops each drop (RESULT@0x00608). |
| `0x424ecf` | unit_capture_items | high | investigation | EBINIT: the summon stone (an ITINIT item) gained when the unit is captured with 性魔術 (BTL@0x028f4, RESULT@0x007c4). |
| `0x4252b7` | unit_capture_levels | med | investigation | EBINIT: the 捕縛 (skill 401) level the capture needs (BTL@0x05ef7). |
| `0x4256a0` | scene_script_ids | high | investigation | SCINIT: the script that plays scene [id], a packed script id (appends' scenes carry the selector in the top byte). SYSTEM4@0x366 copies it into current_scene_script_id and runs it; MENU@0x88a, ROUND@0x290 and AFTERFIGHT@0x206 run it directly. |
| `0x427db0` | scene_offer_contexts | med | investigation | SCINIT, 1 to 9 on 723 scenes: the SCJUMP context that offers the scene (579 of the 606 it names agree). Each scene script's header (SC0000@0x1fe) registers the right-click menu unless it is 4, which no scene has. |
| `0x42a4c0` | area_icons | high | investigation | AMINIT2: column 1 the area's icon on the world map (ALT001A.AGF), ALLMAP@0x14d4 and SHOWALLMAP@0x58. Column 0 is never written; ALLMAP@0x19e2 reads it for the area under the cursor. Columns: 0=cursor, 1=icon. |
| `0x42a588` | area_worlds | high | investigation | AMINIT2: the world map that holds the area, 1 or 2 (the other world). ALLMAP@0x22b sets world_map_id from it; FIELD@0x1c7 and TOWN@0x70 copy it into the saved G[0x118ef7 + G[0x363c]]. |
| `0x42a5ec` | area_kinds | high | investigation | AMINIT2: what entering the area does, 1 FIELD or 2 TOWN (ALLMAP@0xe11..0xe43). Values: 1 Field, 2 Town. |
| `0x42a650` | area_x | high | investigation | AMINIT2: the area's x on the world map (ALLMAP@0x57). |
| `0x42a6b4` | area_y | high | investigation | AMINIT2: the area's y on the world map (ALLMAP@0x63). |
| `0x42a718` | area_up_neighbors | high | investigation | AMINIT2: the area the world-map cursor moves to on up, joy 0 (ALLMAP@0x1251); a hidden area is skipped. |
| `0x42a77c` | area_down_neighbors | high | investigation | AMINIT2: the area the cursor moves to on down, joy 2 (ALLMAP@0x129e). |
| `0x42a7e0` | area_left_neighbors | high | investigation | AMINIT2: the area the cursor moves to on left, joy 3 (ALLMAP@0x12eb). |
| `0x42a844` | area_right_neighbors | high | investigation | AMINIT2: the area the cursor moves to on right, joy 1 (ALLMAP@0x1338). |
| `0x42a8a8` | area_cursor_areas | med | investigation | AMINIT2, on five areas: the area the world-map cursor starts on when the map opens from this one (集落の裏口 -> 北華鏡の集落), ALLMAP@0x27a. |
| `0x42a90c` | town_background | high | investigation | SIINIT: the town's background (MT050A.AGF), surface 0x40 (TOWN@0x785). |
| `0x42a90d` | town_music | high | investigation | SIINIT: the town's music (TOWN@0x344). |
| `0x42a90e` | town_site_pictures | high | investigation | SIINIT: the site's picture (MTS050A.AGF), surface 0x48 + n (TOWN@0x7b5). |
| `0x42a92c` | town_site_x | high | investigation | SIINIT: the site's x (TOWN@0x2fd). |
| `0x42a94a` | town_site_y | high | investigation | SIINIT: the site's y (TOWN@0x310). |
| `0x42a968` | town_site_hot_lefts | high | investigation | SIINIT: the left of the site's hot rectangle (TOWN@0x275). |
| `0x42a986` | town_site_hot_tops | med | investigation | SIINIT: the top of the hot rectangle (TOWN@0x2c0); never written. |
| `0x42a9a4` | town_site_hot_widths | high | investigation | SIINIT: the hot rectangle's width (TOWN@0x29d). |
| `0x42a9c2` | town_site_hot_heights | high | investigation | SIINIT: the hot rectangle's height (TOWN@0x2e8). |
| `0x42a9e0` | town_site_neighbors | high | investigation | SIINIT: the site each of four directions leads to; TOWN@0x704..0x75b follows them to an open site. |
| `0x42ab0e` | shop_sign | med | investigation | SHINIT: which 141x141 cell of the shop sign sheet (texture 0x7c) SHOP draws, 0 to 6 (SHOP@0x02559). |
| `0x42ab0f` | shop_items | high | investigation | SHINIT: the item sold in slot 20 * page + slot (an ITINIT id), 0 for none, negated while it cannot be bought (SHOP refuses it, SHOP@0x00ba0). Page 0 holds unlimited goods, page 1 limited ones; slot 0 is never written. |
| `0x42ab37` | shop_stock_limits | high | investigation | SHINIT: how many of the slot's item the shop sells in all, 0 for unlimited; the stock is the limit minus shop_bought_counts (SHOP@0x01fab). |
| `0x42ab5f` | shop_keeper_picture | high | investigation | SHINIT: the shopkeeper's picture (CS057AA and others), drawn at the bottom right (SHOP@0x02496, 0x02541), 0 for none. |
| `0x42ab60` | shop_background | high | investigation | SHINIT: the shop's background picture (BG002A and others), drawn over the whole screen (SHOP@0x02488, 0x024de), 0 for none. |
| `0x42ab61` | recipe_outputs | high | investigation | ALINIT: the item a recipe makes (an ITINIT id), 0 for no recipe. ALCHEMY lists the recipes whose output is not 0, on the tab of the output's class (ALCHEMY@0x01590, 0x015b4), and ADDITEM adds it at the commit (ALCHEMY@0x01d4d). |
| `0x42af49` | recipe_party_masks | high | investigation | ALINIT: the parties that may craft the recipe, bit k for party k (G[0x363c], 0 to 2) and bit 3 for G[0x363d]; ALCHEMY lists it only when the AND with the party's mask is not 0 (ALCHEMY@0x01a7b). |
| `0x42b331` | recipe_required_flags | high | investigation | ALINIT: four story flags that must all be set for ALCHEMY to list the recipe (ALCHEMY@0x01ab5), 0 for none. |
| `0x42c2d1` | recipe_blocking_flags | high | investigation | ALINIT: two story flags, either of which hides the recipe (ALCHEMY@0x01b0f); only the rod recipes 601 to 603 write one (9048 to 9050, the rod held). |
| `0x42caa1` | recipe_prices | high | investigation | ALINIT: the money one craft costs. ALCHEMY shows it and its multiple by the count (ALCHEMY@0x02621, 0x02658) and charges the bill at the commit (ALCHEMY@0x01bfe). |
| `0x42ce89` | recipe_ingredients | high | investigation | ALINIT: four ingredient items (ITINIT ids), 0 for none; a craft reserves them from ALCHEMY's copy of the inventory (ALCHEMY@0x00b94). |
| `0x42de29` | recipe_ingredient_counts | high | investigation | ALINIT: how many of each ingredient one craft takes; a craft is refused when the count exceeds what is held minus the copies equipped (ALCHEMY@0x01b52). |
| `0x42edc9` | item_growth_kinds | high | investigation | IMINIT: 1 錬成可 (refined in ALCHEMY), 2 成長可 (grows by battles, BTL@0x6017); the descriptions agree on 127 of 128. Values: 1 refine, 2 battle. |
| `0x4314d9` | item_growth_costs | high | investigation | IMINIT: the cost of the next level by the current one: battles for kind 2 (RESULT@0xab2), the refining price for kind 1 (ALCHEMY@0xaab, inferred); 0 marks the last level. |
| `0x44c289` | item_growth_accuracy_bonuses | high | investigation | IMINIT: what each growth level adds to 命中; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x464929` | item_growth_evasion_bonuses | high | investigation | IMINIT: what each growth level adds to 回避; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x47cfc9` | item_growth_physical_attack_bonuses | high | investigation | IMINIT: what each growth level adds to 物攻; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x495669` | item_growth_physical_defense_bonuses | high | investigation | IMINIT: what each growth level adds to 物防; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x4add09` | item_growth_attack_count_bonuses | high | investigation | IMINIT: what each growth level adds to 攻回; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x4c63a9` | item_growth_defense_count_bonuses | high | investigation | IMINIT: what each growth level adds to 防回; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x4dea49` | item_growth_magic_attack_bonuses | high | investigation | IMINIT: what each growth level adds to 魔攻 (not for damage kind 5); SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x4f70e9` | item_growth_magic_defense_bonuses | high | investigation | IMINIT: what each growth level adds to 魔防; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x50f789` | item_growth_magic_power_bonuses | high | investigation | IMINIT: what each growth level adds to 魔法威力; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x558b69` | item_growth_physical_speed_bonuses | high | investigation | IMINIT: what each growth level adds to 肉体戦速; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x571209` | item_growth_mental_speed_bonuses | high | investigation | IMINIT: what each growth level adds to 精神戦速; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x5a1f49` | item_growth_ct_rate_bonuses | high | investigation | IMINIT: what each growth level adds to CT率; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x5ba5e9` | item_growth_element_level_bonuses | high | investigation | IMINIT: what each growth level adds to the attack (weapon) or defense (armour) element level; SETCH@0x754..0x9b3 sums the columns below the item's level into the battle unit. |
| `0x5d2c89` | item_growth_results | high | investigation | IMINIT: the item (ITINIT id) a fully grown one yields; RESULT@0xb54 and ALCHEMY@0x1df1 give it through ADDITEM. |
| `0x5d5399` | item_growth_flags | high | investigation | IMINIT: a flag id; at the last level RESULT@0xba5 and ALCHEMY@0x1e75 set the saved G[0x929 + id − 1] (GAMESTART@0xedf again for a carried-over item). |
| `0x5d7aa9` | arena_match_order | high | investigation | ARINIT: the order of ARENA's match list, seven rows of 13 match ids (ARENA@0x9ef); an id with no name is skipped. |
| `0x5d7b04` | arena_match_kinds | high | investigation | ARINIT: 1 a team match (団体戦), 2 a single fighter's (個人戦, one party member, ARENA@0x822), 3 a special match (特殊戦) (ARENA@0x1138). Values: 1 Team, 2 Single, 3 Special. |
| `0x5d7b68` | arena_match_previous | high | investigation | ARINIT: the match that must be won first (ARENA@0xb0a), by arena_match_wins. |
| `0x5d7bcc` | arena_match_characters | med | investigation | ARINIT, never written: five characters who must have bit 1 of G[0x11967e + character] to enter (ARENA@0xc11). |
| `0x5d7dc0` | arena_match_fees | high | investigation | ARINIT: the entry fee, taken from the saved money G[0x118f0d + G[0x363c]] (ARENA@0x88c..0x8c6). |
| `0x5d7e24` | arena_match_required_flags | high | investigation | ARINIT: three story flags that must be set, n naming G[0x929 + n - 1] (ARENA@0xb52). |
| `0x5d7f50` | arena_match_closing_flags | high | investigation | ARINIT: three story flags that close the match while set, n naming G[0x929 + n - 1] (ARENA@0xbac). |
| `0x5d807c` | arena_match_battle_modes | med | investigation | ARINIT: the battle mode, copied into G[0x63dcda] (ARENA@0xdbe), on which CSINIT branches. |
| `0x5d80e0` | arena_match_enemy_levels | high | investigation | ARINIT: the enemies' level while in the arena (SETEN@0xaf). |
| `0x5d8144` | arena_match_battles | high | investigation | ARINIT: the battle, copied into G[0x276e47] (column 0) and G[0x276e48] (column 1, never written) (ARENA@0xdd4). |
| `0x5d820c` | arena_match_scenes | med | investigation | ARINIT: scenes, column 0 before the battle (ARENA@0x8d0), 1 after a loss and 2 after a win, by G[0x276e99] (BTL@0xa20a, BTL@0xa261). Columns: 0=before, 1=lost, 2=won. |
| `0x5d8338` | arena_match_prizes | high | investigation | ARINIT: the prize money, paid on the first win only (RESULT@0x3a1; 賞金 at ARENA@0x14d0). |
| `0x5d839c` | arena_match_reward_items | med | investigation | ARINIT: two reward items (賞品), added to the battle's dropped items on the first win (RESULT@0x3fc). |
| `0x5d8464` | arena_match_reward_counts | high | investigation | ARINIT: how many of each reward item (RESULT@0x413). |
| `0x5d852f` | quest_prerequisites | high | investigation | QUINIT: the quest that must be cleared first, a slot of the same row (INN@0x944 to 0x980). |
| `0x5d85f7` | quest_parties | med | investigation | QUINIT: the party the quest is for, party + 1; while party_mode is 0 only that party sees it, and 4 would show it only while party_mode is set (INN@0x988 to 0x9e7). |
| `0x5d86bf` | quest_required_flags | high | investigation | QUINIT: story flags, each a flag + 1, that must all be set for the board to offer the quest (INN@0xa05 to 0xa3e). |
| `0x5d8917` | quest_barring_flags | high | investigation | QUINIT: story flags, each a flag + 1, none of which may be set for the board to offer the quest (INN@0xa5f to 0xa98). |
| `0x5d8b6f` | quest_difficulties | high | investigation | QUINIT: the difficulty, 1 to 5, drawn as that many ★ (INN@0xf11 to 0xf34); a slot without one is no quest (INN@0x837). |
| `0x5d8c37` | quest_money_rewards | high | investigation | QUINIT: the money reward, 賞金 (INN@0x1204). QUESTCLEAR@0x10e adds it to the party's money G[0x118f0d + current_party_index], up to 9999999. |
| `0x5d8cff` | quest_reward_items | high | investigation | QUINIT: the item rewards, 賞品, item ids of ITINIT (INN@0x1285 names them from GS[0x2f78 + item]). QUESTCLEAR@0xe8 gives them through ADDITEM. |
| `0x5d8e8f` | quest_reward_counts | high | investigation | QUINIT: how many of each quest_reward_items item (INN@0x12ba, QUESTCLEAR@0xf8). |
| `0x5d901f` | map_object_sheets | high | investigation | MPINIT: the map's object sprite sheet (MO701.AGF), loaded as surface 0x66 (SETMAPTEX@0x132), from which DRAWMAP@0x970 draws the map objects. |
| `0x5d9051` | map_start_floors | high | investigation | MPINIT: the start floor, copied into the saved G[0x36d1] on the first entry (FIELD@0xf5). |
| `0x5d9083` | map_start_sections | high | investigation | MPINIT: the start section, copied into the saved G[0x36d2] (FIELD@0x101). |
| `0x5d90b5` | map_start_x | high | investigation | MPINIT: the start x, copied into the saved G[0x36da] (FIELD@0x10d). |
| `0x5d90e7` | map_start_y | high | investigation | MPINIT: the start y, copied into the saved G[0x36dc] (FIELD@0x119). |
| `0x5d9119` | map_start_facings | med | investigation | MPINIT: the start facing (1 to 7), copied into the saved G[0x36de] (FIELD@0x125). |
| `0x5d914b` | map_encounter_decay_distances | med | investigation | MPINIT: the field's encounter level G[0x36e1] falls by 1 each time the distance walked G[0x36e2] reaches ten times this value (FIELD@0x1575). |
| `0x5d917d` | map_stray_demon_rates | med | investigation | MPINIT: the divisor of the stray-demon (はぐれ魔神) encounter rate; 0 means none (FIELD@0x28df, FIELD@0x296d; debug text at FIELD@0x2982). |
| `0x5d91af` | map_floor_name_plates | high | investigation | MPINIT: each floor's name plate (MPN0101.AGF), columns 1 to 20, loaded as surface 0x69 (SETMAPTEX@0x195) for DRAWMAPH. |
| `0x5d9691` | map_floor_numbers | high | investigation | MPINIT: each floor's number as its name shows it, columns 1 to 20, negative below ground (B1F is -1). No script reads it. |
| `0x5d9b73` | map_block_scripts | high | investigation | RMINIT: the block's map script (MP01B0101.BIN ...), which SETMAPDATA calls (SETMAPDATA@0xce). |
| `0x5d9e61` | map_block_event_scripts | high | investigation | RMINIT: the block's event script (MPEV01B0101.BIN ...), which FIELD calls (FIELD@0x106b). |
| `0x5da14f` | map_block_pictures | high | investigation | RMINIT: the block's picture (MP01B0101A.AGF ...), loaded into surface 0x71 (SETMAPTEX@0x232). |
| `0x5da43d` | map_block_pictures_2 | high | investigation | RMINIT: a second picture of the block (MP01B0101B.AGF ...), surface 0x72 (SETMAPTEX@0x252); 0 for none. |
| `0x5da72b` | map_block_pictures_3 | med | investigation | RMINIT: a picture set on 0x73 (SETMAPTEX@0x27d); SO026A.AGF on 140 blocks of 12 maps. |
| `0x5daa19` | map_block_minimap_pictures | high | investigation | RMINIT: the block's minimap picture (MPS01B0101A.AGF ...) (DRAWMINIMAP@0x12). |
| `0x5dad07` | map_block_battle_backdrops | high | investigation | RMINIT: the backdrop of a battle on the block (MPB007A.AGF ...), set on texture 0x2e (BTL@0x8fb6). |
| `0x5daff5` | map_block_pictures_0 | med | investigation | RMINIT: a picture loaded into surface 0x70, under the block's picture (SETMAPTEX@0x207); on 8 maps. |
| `0x5dc189` | map_block_music | high | investigation | RMINIT: the block's music for each of three variants G[0x36ea] (FIELD@0x66f, play-bgm); FIELD copies it into the saved G[0x2662d5]. Columns: 0=variant_0, 1=variant_1, 2=variant_2. |
| `0x5dca53` | map_block_encounter_sets | high | investigation | RMINIT: the block's encounter set (an ENINIT id) for each of two variants G[0x36eb] (FIELD@0x60d). Columns: 0=variant_0, 1=variant_1. |
| `0x5dd02f` | map_block_enemy_levels | med | investigation | RMINIT: the enemies' base level on the block; SETEN raises it by part of G[0x2756d3]'s lead over it (SETEN@0xbe). |
| `0x5dd31d` | map_block_enemy_level_spreads | med | investigation | RMINIT: the spread of that level: SETEN draws 0 to 2v and takes v off (SETEN@0x10d). |
| `0x5dd60b` | map_block_encounter_levels | high | investigation | RMINIT: the block's encounter level, the column of EEINIT's encounter_chances (FIELD@0x1a30). |
| `0x5dd8f9` | map_character_anchor_x | med | investigation | INIT2 constant 35: what DRAWCH@0x82, DRAWWALK and CARRY take from 0x200 to place a map character. |
| `0x5dd8fa` | map_character_anchor_y | med | investigation | INIT2 constant 80: what DRAWCH@0x82, DRAWWALK and CARRY take from 0x120 to place a map character. |
| `0x5f200f` | land_footstep_sounds | high | investigation | LAINIT: the footstep loop on this land (WALKB.WAV ...); 0 plays WALKA.WAV (FIELD@0x1bf7, DRAWWALK@0x1c4). |
| `0x5f2073` | land_sprite_columns | high | investigation | LAINIT: the column of the leader's walking sheets (TIINIT's G[0xbe2cfd], 25 a character) loaded as surface 0x67 on this land: 1 wading (CPM001AB), 9 a mine cart (CPM001AJ) (FIELD@0x1bb8, DRAWCH@0x37). |
| `0x5f20d7` | land_speed_changes | high | investigation | LAINIT: the walking speed's change in percent; the step is max(1, 50 * (100 + value) / 1000) (FIELD@0x1b6d, DRAWWALK@0x153). |
| `0x5f213b` | land_drift_x | high | investigation | LAINIT: the x of the drift the land adds to every step, times speed / 100 (FIELD@0x1f61). |
| `0x5f219f` | land_drift_y | high | investigation | LAINIT: the y of that drift (FIELD@0x1f7d). |
| `0x5f2203` | land_drift_walkable | high | investigation | LAINIT: 1 lets the player walk against the drift; 0 drops the part of a step that goes against it (FIELD@0x1f0a). |
| `0x5f232f` | land_encounter_classes | med | investigation | LAINIT: replaces the section's own value (RMINIT's G[0x5dd60b]) as the column, 0 to 9, of EEINIT's G[0x63dcfe] that FIELD@0x1a70 reads (FIELD@0x1a13). |
| `0x5f2393` | object_kind_sprite_x | high | investigation | OBINIT: x of the kind's rectangle in the map's object sheet (DRAWMAP@0x930). |
| `0x5f277b` | object_kind_sprite_y | high | investigation | OBINIT: y of that rectangle (DRAWMAP@0x937). |
| `0x5f2b63` | object_kind_widths | high | investigation | OBINIT: the rectangle's width (DRAWMAP@0x93e). |
| `0x5f2f4b` | object_kind_heights | high | investigation | OBINIT: the rectangle's height (DRAWMAP@0x945). |
| `0x5f3333` | object_kind_blend_modes | high | investigation | OBINIT: the blend mode, 1 additive on the glow rings (DRAWMAP@0x988). Values: 1 Additive. |
| `0x5f371b` | object_kind_depth_offsets | high | investigation | OBINIT2: added to the object's y to sort it among the sprites; -9999 and 9999 put it under or over all (DRAWMAP@0xf5, DRAWMAP@0x28a). |
| `0x5f3b03` | object_kind_wall_point_counts | high | investigation | OBINIT2: the number of points of the kind's wall polygon (FIELD@0x1191). |
| `0x5f3eeb` | object_kind_wall_point_x | high | investigation | OBINIT2: the wall points' x from the object's position; FIELD@0x11e8 adds the position and writes the AGERC wall tables. |
| `0x60c58b` | object_kind_wall_point_y | high | investigation | OBINIT2: the wall points' y (FIELD@0x1210). |
| `0x624c2b` | object_kind_next_kinds | high | investigation | OBINIT: the kind an object becomes when its switch is on, -1 to remove it; EVOLVEOBJ@0x55 writes it into the saved object kinds G[0xbb843 ...]. |
| `0x625013` | object_kind_change_sounds | high | investigation | OBINIT: the sound that change plays (SE001.WAV ...), EVOLVEOBJ@0xba. |
| `0x6253fc` | treasure_items | high | investigation | TRINIT: the item of each of a treasure's five slots (an ITINIT id), 0 where the slot gives money; GETITEM adds it with ADDITEM (GETITEM@0x0001d, 0x00109). |
| `0x63174c` | treasure_counts | high | investigation | TRINIT: a slot's count, or the money it gives where its item is 0; GETITEM adds the money to the party's purse up to 9,999,999 (GETITEM@0x0012f). |
| `0x63da9d` | encounter_entry_weights | high | investigation | ENINIT: the entry's weight in SETENP's draw (SETENP@0x314); weight 0 is skipped. |
| `0x63db01` | encounter_entry_gauge_lows | high | investigation | ENINIT: the low end of the entry's window, plus 16: it may be drawn when the field gauge G[0x36e1] >= value - 16; 0 for always (SETENP@0x2c7). |
| `0x63db65` | encounter_entry_gauge_highs | high | investigation | ENINIT: the high end of that window, plus 16: G[0x36e1] <= value - 16 (SETENP@0x2ea). |
| `0x63dbc9` | encounter_entry_formations | high | investigation | ENINIT: the entry's two formations (ESINIT ids), into G[0x276e47] and G[0x276e48] (SETENP@0x39b); the second is placed five rows further (SETENP@0x84e). Columns: 0=front, 1=back. |
| `0x63dc92` | formation_slot_units | high | investigation | ESINIT: the slot's enemy, an EBINIT unit (into G[0x370816], SETENP@0x553); 0 for an empty slot. |
| `0x63dc9a` | formation_slot_first_columns | high | investigation | ESINIT: the first column (1-based) the enemy prefers on the 5 x 5 field; 0 for any (SETENP@0x55f). |
| `0x63dca2` | formation_slot_last_columns | high | investigation | ESINIT: the last column it prefers; below the first, the first alone (SETENP@0x588). |
| `0x63dcaa` | formation_slot_first_rows | high | investigation | ESINIT: the first row (1-based) the enemy prefers; 0 for any (SETENP@0x5cf). |
| `0x63dcb2` | formation_slot_last_rows | high | investigation | ESINIT: the last row it prefers (SETENP@0x5f8). |
| `0x63dcba` | formation_slot_chances | high | investigation | ESINIT: the percent chance the slot's enemy appears; 0 for always (SETENP@0x4c6). |
| `0x63dcc2` | formation_slot_required_flags | high | investigation | ESINIT: a story flag that must be on for the slot, G[0x929 + flag - 1] (SETENP@0x4ef). |
| `0x63dcca` | formation_slot_barring_flags | high | investigation | ESINIT: a story flag that must be off for the slot (SETENP@0x521). |
| `0x63dcd2` | formation_slot_links | high | investigation | ESINIT: the slot (1-based) whose battler this one is linked to: G[0xc1ca30 + battler] gets it (SETENP@0x487). |
| `0x63dcdb` | scripted_party_characters | high | investigation | CSINIT: the character in a battle slot (a CHINIT id), 0 for none; BTL copies slots 0 to 4 to G[0x36f9f8] and 5 and 6 to G[0x36f9fd] (BTL@0x00130). |
| `0x63dce2` | scripted_party_columns | high | investigation | CSINIT: the slot's start column, 1 to 5; SETENP places the character at x = column - 1 (SETENP@0x0018f). |
| `0x63dce9` | scripted_party_rows | high | investigation | CSINIT: the slot's start row, 1 to 3; SETENP places the character at y = row - 4 (SETENP@0x001a4). |
| `0x63dcf0` | scripted_party_required_flags | high | investigation | CSINIT: a story flag that must be set for BTL to keep the slot (BTL@0x08247); no case writes it. |
| `0x63dcf7` | scripted_party_blocking_flags | high | investigation | CSINIT: a story flag that makes BTL drop the slot when set (BTL@0x08279); no case writes it. |
| `0x63dcfe` | encounter_chances | high | investigation | EEINIT: the percent chance of a battle at a field roll, by distance band (row: G[0x36e3] / 20 below 10, 20, 40, 70, or more) and encounter level 0 to 9 (column) (FIELD@0x1a70). |
| `0x63dd30` | puzzle_width | high | investigation | PZINIT: the puzzle grid's width (CARRY@0x790). |
| `0x63dd31` | puzzle_height | high | investigation | PZINIT: the puzzle grid's height (CARRY@0x773). |
| `0x63dd32` | puzzle_grid | high | investigation | PZINIT: the grid by row and column, from 1: 0 no cell, 1 floor, 10 + k a goal for a block of kind k (CARRY@0x158). |
| `0x63df16` | puzzle_block_count | high | investigation | PZINIT: the number of blocks. |
| `0x63df17` | puzzle_block_x | high | investigation | PZINIT: each block's start column; SETMAPDATA@0x174 copies it into puzzle_saved_block_x the first time. |
| `0x63df1f` | puzzle_block_y | high | investigation | PZINIT: each block's start row, copied into puzzle_saved_block_y the first time. |
| `0x63df27` | puzzle_block_kinds | high | investigation | PZINIT: the block's kind, which a goal of 10 + kind takes (CARRY@0x158). |
| `0x63df2f` | puzzle_block_flags | low | investigation | PZINIT: a flag CARRY@0x47b tests for each push direction; not read further. |
| `0x63df39` | item_sort_keys | high | investigation | ITINIT: a sort number in steps of ten, on items only; the item lists sort by it (sort-indices-by-key-sum at ALCHEMY@0x149a, CHMODE1@0x1734, SHOP@0x1dcd). |
| `0x640649` | item_ranks | high | investigation | ITINIT: the rank, 1 to 7 = E, D, C, B, A, S, M (the descriptions name it). CHMODE1@0x1caf refuses an item while the character's rank in its class is lower. Values: 1 E, 2 D, 3 C, 4 B, 5 A, 6 S, 7 M. |
| `0x642d59` | item_categories | high | investigation | ITINIT: the category, 1 to 59 (weapons 1..12, armour 16..22, 26 重要, 27 消費 … 35 魚, magic schools 36..49, art schools 51..59). ITMES@0x0 dispatches on it; DRAWTIP@0x273 draws its icon. Values: 1 杖, 2 投石, 3 短剣, 5 突剣, 6 剣, 7 拳, 8 連接, 9 双剣, 10 大剣, 11 槍, 12 弓, 16 衣服, 17 中装鎧, 18 重装鎧, 19 魔導鎧, 20 盾, 22 装飾, 26 重要, 27 消費, 29 支配率操作, 30 合成, 31 売却専用, 33 成長, 34 餌, 35 魚. |
| `0x645469` | item_classes | high | investigation | ITINIT: the class, 1 to 55, an index into AFINIT's class arrays (1 重要, 2 物品, 3 非戦, 4..6 weapons, 7..13 armour, 21..40 magic, 41..55 arts). ADDITEM@0x78 treats class 1 as a key item; GAMECLEAR@0x3e1 carries over the counts of classes over 1. Names: `class_names`, element 0 naming 0. |
| `0x647b79` | item_damage_kinds | med | investigation | ITINIT: 1 physical, 2 magic, 3 mind check, 4 buff or special, 5 戦意 attack. CALCDAMAGE@0x213 and BTL@0x332d branch on it; with 5 DRAWTIP@0x20f names stat 6 戦意. Values: 1 physical, 2 magic, 3 mind_check, 4 special, 5 fighting_spirit. |
| `0x64a289` | item_once_per_battle_flags | high | investigation | ITINIT: 1 on five 軍配 arts usable once a battle (CHECKACT@0x869, CHMODE3@0x17a3). |
| `0x64c999` | item_charge_flags | med | investigation | ITINIT: 1 on 197 attack spells whose charge time and damage scale with the caster's charge level G[0xc1c418] (BTL@0x33f1, CALCDAMAGE@0x7de). |
| `0x64f0a9` | item_attack_elements | high | investigation | ITINIT: the attack element 1..17, the affinity matrix's column (CALCDAMAGE@0x179); names in attack_element_names. Names: `attack_element_names`, element 0 naming 0. |
| `0x6517b9` | item_attack_element_levels | high | investigation | ITINIT: the attack element's level 1..5 ("N属性"); each level over the defender's adds 20 to the affinity (CALCDAMAGE@0x179..0x20c). |
| `0x653ec9` | item_defense_elements | high | investigation | ITINIT: the armour's defense element 2..15, the affinity matrix's row; SETCH@0xa33 takes the character's own without armour. Names: `defense_element_names`, element 0 naming 0. |
| `0x6565d9` | item_defense_element_levels | high | investigation | ITINIT: the defense element's level 1..4 (SETCH@0xa33, CALCDAMAGE@0x179). |
| `0x660219` | item_owners | high | investigation | ITINIT: the characters (CHINIT ids) who own the item; CHMODE1@0x19f4 refuses it unless one is in the party (bit 0 of G[0x11967e + char]). |
| `0x754459` | item_haishera_flags | high | investigation | ITINIT: 1 on ten ハイシェラ items, refused unless the character in G[0x266384] is present (CHMODE1@0x1a82); PUTHAI reads it. |
| `0x756b69` | item_exclusive_users | high | investigation | ITINIT: the characters (CHINIT ids) the item is exclusive to (専用); CHMODE1@0x1b80 refuses it to anyone else. |
| `0x84ada9` | item_female_only_flags | med | investigation | ITINIT: 1 on 17 items for women; CHMODE1@0x1acb refuses them when G[0x3788ff + G[0xc1a739 + char]] is not 2 (inferred: the sex). |
| `0x859809` | item_required_skills | high | investigation | ITINIT: a skill (SSINIT id) the user must have; CHMODE1@0x1b08 checks the character's and the class's levels and its switch; CALCREVISE@0xa9 reads it. |
| `0x85bf19` | item_cursed_flags | high | investigation | ITINIT: 1 on five cursed items: CHMODE1@0x1e7d refuses to take one off ("は外すことが出来ない…"); ADDILL@0x486, CALCILL@0x113 and SHOP@0x168a read it. |
| `0x85e629` | item_reaches | high | investigation | ITINIT: 1 ground, 2 melee, 3 ranged; against a flying defender (skill 373 or 379) they do 0, ×0.8 and ×1.5 (SETSSDAMAGE@0x24..0xb3). Values: 1 ground, 2 melee, 3 ranged. |
| `0x860d39` | item_target_kinds | high | investigation | ITINIT: 1 enemies, 2 allies, 3 a growth item, 5 a summon; CALCDAMAGE@0x32 takes the recovery path for anything but 1; SELTARGET reads it. Values: 1 enemies, 2 allies, 3 growth_item, 5 summon. |
| `0x863449` | item_area_widths | high | investigation | ITINIT: the area's width, the W of "範囲W×H" in every description that gives one. |
| `0x865b59` | item_area_heights | high | investigation | ITINIT: the area's height, the H of "範囲W×H". |
| `0x868269` | item_charge_times | med | investigation | ITINIT: the time before the action takes effect (name inferred); BTL@0x3321 subtracts the unit's 肉体戦速 or 精神戦速, down to item_min_charge_times. |
| `0x86a979` | item_min_charge_times | med | investigation | ITINIT: the least charge time (BTL@0x3321). |
| `0x86d089` | item_recovery_times | med | investigation | ITINIT: the time after the action (name inferred), down to item_min_recovery_times (BTL@0x4be7). |
| `0x86f799` | item_min_recovery_times | med | investigation | ITINIT: the least recovery time (BTL@0x4be7). |
| `0x871ea9` | item_condition_levels | high | investigation | ITINIT: condition levels by ILINIT condition id, positive inflicts and negative cures (127 女神の滴 −1 on eight); BTL, CALCILL@0xa7, USESKILLSUB read it. Column k is record k of `condition`. |
| `0x904669` | item_equip_condition_levels | high | investigation | ITINIT: conditions, by ILINIT id and level, that an equipment keeps on its wearer (CALCREVISE@0x41d, SETCH@0xe89). Column k is record k of `condition`. |
| `0x996e29` | item_accuracy_bonuses | high | investigation | ITINIT: what an equipped item adds to 命中; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1acc6] (not saved). |
| `0x999539` | item_evasion_bonuses | high | investigation | ITINIT: what an equipped item adds to 回避; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ace4] (not saved). |
| `0x99bc49` | item_physical_attack_bonuses | high | investigation | ITINIT: what an equipped item adds to 物攻; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aa50] (not saved). |
| `0x99e359` | item_physical_defense_bonuses | high | investigation | ITINIT: what an equipped item adds to 物防; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aa6e] (not saved). |
| `0x9a0a69` | item_attack_count_bonuses | high | investigation | ITINIT: what an equipped item adds to 攻回 (attacks per action); SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aac8] (not saved). |
| `0x9a3179` | item_defense_count_bonuses | high | investigation | ITINIT: what an equipped item adds to 防回 (defenses); SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aae6] (not saved). |
| `0x9a5889` | item_magic_attack_bonuses | high | investigation | ITINIT: what an equipped item adds to 魔攻 (戦意 for a damage kind 5 action, which SETCH skips); SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aa8c] (not saved). |
| `0x9a7f99` | item_magic_defense_bonuses | high | investigation | ITINIT: what an equipped item adds to 魔防; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aaaa] (not saved). |
| `0x9aa6a9` | item_magic_power_bonuses | high | investigation | ITINIT: what an equipped item adds to 魔法威力 (CALCDAMAGE@0x707); SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ad20] (not saved). |
| `0x9acdb9` | item_physical_resistance_bonuses | high | investigation | ITINIT: what an equipped item adds to 肉体抵抗 (CALCILL@0x68b); SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ab04] (not saved). |
| `0x9af4c9` | item_mental_resistance_bonuses | high | investigation | ITINIT: what an equipped item adds to 精神抵抗 (CALCILL@0x649); SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ab22] (not saved). |
| `0x9b1bd9` | item_physical_speed_bonuses | high | investigation | ITINIT: what an equipped item adds to 肉体戦速; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ab40] (not saved). |
| `0x9b42e9` | item_mental_speed_bonuses | high | investigation | ITINIT: what an equipped item adds to 精神戦速; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ab5e] (not saved). |
| `0x9b69f9` | item_luck_bonuses | high | investigation | ITINIT: what an equipped item adds to 運; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ab7c] (not saved). |
| `0x9b9109` | item_ct_rate_bonuses | high | investigation | ITINIT: what an equipped item adds to CT率; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1ad02] (not saved). |
| `0x9bb819` | item_hp_amounts | high | investigation | ITINIT: HP an action restores (positive) or costs (negative); CALCDAMAGE@0x4c caps a restore at 99999, CHMODE3@0x1187 shows a cost. |
| `0x9bdf29` | item_mp_amounts | high | investigation | ITINIT: MP restored or spent ("MP10" is −10; CALCDAMAGE@0x4c, CHMODE3@0x1187). |
| `0x9c0639` | item_tp_amounts | high | investigation | ITINIT: TP restored or spent (身妖舞's "TP14" is −14). |
| `0x9c2d49` | item_fs_amounts | high | investigation | ITINIT: 戦意 (FS) restored; CALCDAMAGE@0x160 sets G[0x276e95] to minus it. |
| `0x9c5459` | item_control_rate_changes | med | investigation | ITINIT: the change to the dungeon's 支配率, ±5 to 15; USESKILL@0x0 adds it to the saved G[0x36e1] and keeps that at 15 or less. |
| `0x9c7b69` | item_unread_marks | high | investigation | ITINIT: -1 on 205 consumables and materials (ids 101..625), the array between item_control_rate_changes and item_max_hp_bonuses; no script reads it. |
| `0x9ca279` | item_max_hp_bonuses | high | investigation | ITINIT: what an equipped item adds to 最大HP; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1a9ba] (not saved). |
| `0x9cc989` | item_max_mp_bonuses | high | investigation | ITINIT: what an equipped item adds to 最大MP, where not 0; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1a9f6] (not saved). |
| `0x9cf099` | item_max_tp_bonuses | high | investigation | ITINIT: what an equipped item adds to 最大TP, where not 0; SETCH@0x4f9..0x729 adds it into the battle unit's G[0xc1aa32] (not saved). |
| `0x9d3eb9` | item_random_amounts | high | investigation | ITINIT: a random part; random-modulo of it is added to the amount (CALCDAMAGE@0x74, USESKILLSUB@0x32a). |
| `0x9d65c9` | item_animations | high | investigation | ITINIT: the battle animation; BTL@0x1b89 puts it in G[0xc25964] and calls ANIME. Most records hold their own id. |
| `0x9d8cd9` | item_buy_prices | high | investigation | ITINIT: the buy price (SHOP@0x2b6b draws it; a purchase total stays within 9999999). |
| `0x9db3e9` | item_sell_prices | high | investigation | ITINIT: the sell price, a quarter of the buy price; 0 cannot be sold (SHOP@0x1213, @0x2db8, @0x3307). |
| `0x9ddaf9` | item_menu_use_masks | med | investigation | ITINIT: where the menu offers the item: bit 0 while G[0x36e5] is set (FIELD@0xc5 sets it; inferred: on the field), bit 1 while it is not (CHMODE1@0x3f4, CHMODE3@0x2e3). Battle use does not check it. |
| `0x9e0209` | item_granted_skills | high | investigation | ITINIT: a skill (SSINIT id) an equipment gives its wearer; SETCH@0xafb adds it to the unit's G[0xc1ca4e] row. |
| `0x9e2919` | item_granted_skill_levels | high | investigation | ITINIT: the level, 1 to 5, of item_granted_skills (SETCH@0xafb). |
| `0x9e5029` | school_tree_actions | high | investigation | SKINIT: a school's skill tree (school = an AFINIT item_class), the action (ITINIT id) in each of its 15 slots, 3 x tier + line; 0 an empty slot. CHMODE3@0x2471 draws 5 tiers by 3 lines; CHECKSKILL@0x109 learns them. Columns: 0=tier_1_line_1, 1=tier_1_line_2, 2=tier_1_line_3, 3=tier_2_line_1, 4=tier_2_line_2, 5=tier_2_line_3, 6=tier_3_line_1, 7=tier_3_line_2, 8=tier_3_line_3, 9=tier_4_line_1, 10=tier_4_line_2, 11=tier_4_line_3, 12=tier_5_line_1, 13=tier_5_line_2, 14=tier_5_line_3. |
| `0x9e53ad` | action_tree_slots | high | investigation | SKINIT, keyed by action (ITINIT id): its slot in its school's tree, 15 x school + 3 x tier + line, the index of the saved learned bits G[0x13ee1a] and proficiency G[0x154daa]. ADDSKILL@0x213, CHECKSKILL@0x222, BTL@0x4936, CHECKACT@0x7eb. |
| `0x9e7abd` | action_prerequisite_actions | high | investigation | SKINIT, keyed by action: up to three actions that must be mastered (slot bits 0 and 1) before it can be learned. ADDSKILL@0x1f1, CHECKSKILL@0x200; CHMODE3@0x2563 draws the links. |
| `0x9eefed` | action_required_flags | high | investigation | SKINIT, keyed by action: a story flag it needs, set when G[0x929 + flag - 1] is 1. ADDSKILL@0x1a2, CHECKSKILL@0x1b1, CHMODE3@0x1b20. |
| `0x9f16ff` | condition_resistances | high | investigation | ILINIT: which resistance each level is rolled against: 3 mental (G[0xc1ab22]), 4 physical (G[0xc1ab04]); another value is not rolled or kept (CALCILL@0x523, @0x630). Values: 3 mental, 4 physical. |
| `0x9f182b` | condition_affinity_columns | high | investigation | ILINIT: the column of the defender's AFINIT element_affinities row G[0xc0b56d + 20 x element] that scales the chance of each level (CALCILL@0x5ec). |
| `0x9f1957` | condition_durations | high | investigation | ILINIT: the duration of each level, -1 until cured; ADDILL@0x3ed copies it into the saved G[0x11feca], CALCILL@0xae3 into the battle unit's G[0xc1bbc6]; FIELD@0x1756 counts it down on the map. |
| `0x9f1a83` | condition_hit_modifiers | med | investigation | ILINIT: what each level adds to hit, G[0xc1acc6] (SETILLREVISE@0x198); 命中, 命低, 呪い. |
| `0x9f1baf` | condition_evasion_modifiers | med | investigation | ILINIT: what each level adds to evasion, G[0xc1ace4] (SETILLREVISE@0x1af); 回避, 回低, 麻痺, 呪い. |
| `0x9f1cdb` | condition_physical_attack_modifiers | med | investigation | ILINIT: the percent each level adds to physical attack, G[0xc1aa50] (SETILLREVISE@0x9b, @0x22d); 物攻, 攻低. |
| `0x9f1e07` | condition_physical_defense_modifiers | med | investigation | ILINIT: the percent each level adds to physical defence, G[0xc1aa6e] (SETILLREVISE@0xb2); 物防, 防低. |
| `0x9f1f33` | condition_attack_count_modifiers | med | investigation | ILINIT: what each level adds to the attack count, G[0xc1aac8] (SETILLREVISE@0xf7); 攻回. |
| `0x9f205f` | condition_defense_count_modifiers | med | investigation | ILINIT: what each level adds to the defence count, G[0xc1aae6] (SETILLREVISE@0x10e); 防回. |
| `0x9f218b` | condition_magic_attack_modifiers | med | investigation | ILINIT: the percent each level adds to magic attack, G[0xc1aa8c] (SETILLREVISE@0xc9); 魔攻, 攻低. |
| `0x9f22b7` | condition_magic_defense_modifiers | med | investigation | ILINIT: the percent each level adds to magic defence, G[0xc1aaaa] (SETILLREVISE@0xe0); 魔防, 防低. |
| `0x9f23e3` | condition_magic_power_modifiers | med | investigation | ILINIT: what each level adds to magic power, G[0xc1ad20] (SETILLREVISE@0x1dd); 魔威. |
| `0x9f250f` | condition_physical_resistance_modifiers | med | investigation | ILINIT: what each level adds to physical resistance, G[0xc1ab04], which CALCILL rolls physical conditions against (SETILLREVISE@0x125); 肉抵. |
| `0x9f263b` | condition_mental_resistance_modifiers | med | investigation | ILINIT: what each level adds to mental resistance, G[0xc1ab22], which CALCILL rolls mental conditions against (SETILLREVISE@0x13c); 精抵. |
| `0x9f2767` | condition_physical_speed_modifiers | med | investigation | ILINIT: what each level adds to physical speed, G[0xc1ab40] (SETILLREVISE@0x153); 肉速, 速低, 束縛. |
| `0x9f2893` | condition_mental_speed_modifiers | med | investigation | ILINIT: what each level adds to mental speed, G[0xc1ab5e] (SETILLREVISE@0x16a); 精速, 速低, 束縛. |
| `0x9f29bf` | condition_luck_modifiers | med | investigation | ILINIT: what each level of 運 adds to luck; SETILLREVISE@0x181 sums it and applies the sum to nothing. |
| `0x9f2aeb` | condition_critical_modifiers | med | investigation | ILINIT: what each level adds to critical hits, G[0xc1ad02] (SETILLREVISE@0x1c6); 痛打, 高揚, 呪い. |
| `0x9f2c17` | condition_hp_changes | high | investigation | ILINIT: the HP each level gives or takes each turn: G[0xc1ad3e] in battle (SETILLREVISE@0x1f4), the saved HP G[0x11980e] on the map (FIELD@0x16dd); 猛毒, 再生. |
| `0x9f2d43` | condition_mp_changes | high | investigation | ILINIT: the MP each level gives or takes each turn: G[0xc1ad5c] in battle (SETILLREVISE@0x206), the saved MP G[0x1198d6] on the map (FIELD@0x16fd); 魔再, 衰弱. |
| `0x9f2e6f` | condition_tp_changes | high | investigation | ILINIT: the TP each level gives or takes each turn: G[0xc1ad7a] in battle (SETILLREVISE@0x218), the saved TP G[0x11999e] on the map (FIELD@0x171d); 技再, 衰弱. |
| `0x9f2f9b` | condition_gauge_drains | high | investigation | ILINIT: what each level takes from the battle unit's G[0xc1a960] every 10 clock ticks, to no less than 1 (BTL@0x211c); 強酸, 恐怖, 衰弱. |
| `0x9f30c7` | condition_icons | med | investigation | ILINIT: the condition's icon; conditions that share one are drawn once (DRAWENLIST@0x7d). |
| `0x9f3103` | condition_polarities | high | investigation | ILINIT: 1 beneficial, which skill 7 破術 takes away (CALCILL@0x63, CHECKSS2@0x117); 2 an ailment, which marks the character on the map (DRAWMAPF@0xd1). The table's leader. Values: 1 beneficial, 2 ailment. |
| `0x9f313f` | condition_boss_proof_flags | med | investigation | ILINIT: 1 when the condition fails on a unit whose G[0x37989f + G[0xc1a8ca + unit]] is set (CALCILL@0x5cf). |
| `0x9f317b` | condition_cure_bits | med | investigation | ILINIT: which cures clear the condition, as bits: bit 1 a knock-out (BTL@0x5d41) and RECOVER (RECOVER@0x6b); bit 0 RECOVER when G[0x36e5] is set. |
| `0x9f31b9` | skill_append_numbers | high | investigation | SSINIT: the append whose name-plate sheet holds a kind-1 skill's plate (3 on the $3$ skills). BTL@0x91cf copies the 154 x 18 plate from surface 0xac + this value, the SO036 sheet of that slot of the chain G[0xc781f0]. |
| `0x9f3411` | skill_sort_keys | high | investigation | SSINIT: a skill's sort number, in steps of ten; sort-indices-by-key-sum at CHECKSS@0x274, COMMITSS@0x2a, INFO6@0x7a5. Unwritten on 8 skills. |
| `0x9f3669` | skill_unlock_scenes | med | investigation | SSINIT: up to three scenes (scene id + 1); INFO6@0x7e5-0x88a lists the skill only when column 0 is 0 or one of them is set in the shared profile's seen-scene cells G[0x25febb]. Only column 0 is written. |
| `0x9f3d71` | skill_kinds | high | investigation | SSINIT: when a skill acts: 1 the attacker's (CHECKSS@0x95), 2 the defender's (CHECKSS@0xbd), 4 passive, its condition row added to the unit's (CALCREVISE@0x347, SETCH@0xce8), 6 a support skill run on the battle clock (BTL@0x2353); 3 and 5 not read. The table's leader. Values: 1 attacker, 2 defender, 4 passive, 6 support. |
| `0x9f3fc9` | skill_attack_only_flags | med | investigation | SSINIT: 1 when the skill acts only on an attack, an action whose ITINIT G[0x647b79] is 1 or 2 (CHECKSS@0xe2); COMMITSS@0x86 takes it back and refunds its costs when the action does not land. |
| `0x9f4221` | skill_effects | high | investigation | SSINIT: a BTANINIT effect id played when the skill triggers: BTL@0x3e47 and COMMITSS@0x355 put it in G[0xc259fc + unit], SETEFFECT@0x9b runs the BTANINIT chain with it. |
| `0x9f4479` | skill_effect_sides | high | investigation | SSINIT: whose unit shows skill_effects: 1 the attacker (BTL@0x3e47), 2 the defender (COMMITSS@0x355). Values: 1 attacker, 2 defender. |
| `0x9f46d1` | skill_hp_costs | med | investigation | SSINIT: the HP cost of each level; CHECKSS@0x30d keeps the skill only when HP after it stays at least 1, COMMITSS@0xd4 takes it. No record writes it. |
| `0x9f5289` | skill_mp_costs | high | investigation | SSINIT: the MP cost of each level, negative; CHECKSS@0x334 checks it, COMMITSS@0xf4 takes it, CHMODE2@0x2c72 shows it. |
| `0x9f5e41` | skill_tp_costs | high | investigation | SSINIT: the TP cost of each level, negative; CHECKSS@0x362 checks it, CHMODE2@0x2cc4 shows it. |
| `0x9f69f9` | skill_trigger_chances | high | investigation | SSINIT: the trigger chance of each level in percent; CHECKSS@0x148 drops the skill when random(100) is at least it. |
| `0x9f75b1` | skill_amounts | med | investigation | SSINIT: the amount of each level, each skill's own: 祈り heals that percent of maximum HP (BTL@0x2413), 魔術結界 scales magic damage to that percent (SETSSDAMAGE@0x1e7), a support action's power (BTL@0x25b6). |
| `0x9f8169` | skill_support_modes | med | investigation | SSINIT, support skills: 1 an attack whose power comes from the supporter's G[0x1196e2] (BTL@0x2575-0x2605), 2 the amount negated, a heal (BTL@0x260b). Values: 1 attack, 2 heal. |
| `0x9f8d21` | skill_always_on_flags | high | investigation | SSINIT: 1 when the player cannot switch the level off; CHMODE2@0x1310 refuses the toggle, CALCREVISE@0x302 and SETCH@0xcb5 ignore the saved switch G[0x1303ba]. |
| `0x9f98d9` | skill_condition_levels | high | investigation | SSINIT: the ILINIT condition levels each skill level gives, cell 300 x skill + 60 x level + condition; read for passive skills only (CALCREVISE@0x392 rebuilds the saved floor G[0x11e75a] from it, SETCH@0xd33, SETEN@0xe8a). |
| `0xa257f9` | skill_support_actions | high | investigation | SSINIT: the ITINIT action a support skill performs (水弾支援 9991); BTL@0x2527-0x2570 puts it in G[0x276e55]. |
| `0xa263b1` | skill_support_periods | high | investigation | SSINIT: a support skill's period: it acts when the battle clock G[0x276e4d] is a multiple of it (BTL@0x2393-0x23c5). |
| `0xa26f6c` | routine_step_kinds | high | investigation | RTINIT: each step's rule, 1 to 13, 51 to 53, 101 to 103, 201 to 203 and 301 to 306 for RTN001 to RTN306; 0, or a rule with no script, ends the routine (RTN@0x141). |
| `0xa332bc` | routine_step_chances | high | investigation | RTINIT: the percent chance the step runs (RTN@0x22a draws 0 to 99). |
| `0xa3f60c` | routine_step_tactic_masks | high | investigation | RTINIT: for a party battler, the tactics that skip the step: bit t set skips it under tactic t, column 1 of the character's G[0x16ad3a] tactics (RTN@0x1fa, check-bit). |
| `0xa4b95c` | routine_step_actions | high | investigation | RTINIT: the step's action: below 6 a slot of the unit's six actions G[0x36fa00], otherwise an ITINIT id (RTN003@0x0); a skill kind for rules 51 and 53 (RTN051@0x86); an item for 301 to 306. |
| `0xa57cac` | routine_step_args | high | investigation | RTINIT: the rule's first argument: a gauge (1 HP, 2 MP, 3 TP, 4 the gauge G[0xc1a960]) for rules 4 to 6, 10, 11, 101 to 103, 304 to 306; a unit for 7; a battle-clock time for 12 and 303 (RTN012@0x10). |
| `0xa63ffc` | routine_step_percents | high | investigation | RTINIT: the percent of that gauge a rule tests against, at or above (5, 10) or at or below (6, 11, 101 to 103, 304 to 306) (RTN010@0x125, RTN011@0x125). |
| `0xa7034c` | routine_step_target_counts | high | investigation | RTINIT: for rules 103 and 306, how many targets must meet the gauge threshold (RTN103@0x29c). |
| `0xa889ec` | routine_step_flags | high | investigation | RTINIT: a story flag the step needs, G[0x929 + flag - 1] (RTN@0x181); no routine sets one. |
| `0xa94d3c` | routine_step_limits | high | investigation | RTINIT: how many times the step may run in a battle, against the count G[0xc1c436] (RTN@0x1b1). |
| `0xaa108d` | title_orders | high | investigation | TIINIT: the order CHECKTITLE@0x00023 tries a character's 20 titles in. |
| `0xaa185d` | title_required_levels | high | investigation | TIINIT: the level a title needs (CHECKTITLE@0x000b1); -1 never by level. For a class 3 or 4 title the master's level counts. |
| `0xaa202d` | title_required_titles | high | investigation | TIINIT: a title that must be earned first (CHECKTITLE@0x00125). |
| `0xaa27fd` | title_required_flags | high | investigation | TIINIT: a story flag the title needs, value - 1 of G[0x929] (CHECKTITLE@0x00157). |
| `0xaa2fcd` | title_excluding_flags | high | investigation | TIINIT: a story flag that stops the title, value - 1 of G[0x929] (CHECKTITLE@0x00190). No record writes it. |
| `0xaa379d` | title_classes | high | investigation | TIINIT: the title's class; CHMODE1@0x029bf draws its tag, Master for 0 and 3. ADDTITLE@0x00001 announces any but class 3 and grants 特技 only below 3. Values: 1 使徒, 2 召喚, 3 Master, 4 支援. |
| `0xaa3f6d` | title_master_ids | high | investigation | TIINIT: the title's master (契約者), a CHINIT character; CHECKTITLE@0x000f4 takes the master's level for a class 3 or 4 title. |
| `0xaa473d` | title_equip_kind_flags | high | investigation | TIINIT: item kinds the title adds to the unit's; CHANGETITLE@0x0013f unequips the saved slots the new mask does not allow. |
| `0xaa4f0d` | title_special_skill_ids | high | investigation | TIINIT: the 特技 the title grants, ids below 60; ADDTITLE@0x000e8 runs ADDSKILL for each, raising the saved G[0x119eb2 + 60 x character + id]. |
| `0xaa955d` | title_special_skill_levels | high | investigation | TIINIT: each granted 特技's level (ADDTITLE@0x00136). Values: 1 E, 2 D, 3 C, 4 B, 5 A, 6 S, 7 M. |
| `0xaadbad` | title_skill_levels | high | investigation | TIINIT: skill levels the title adds, column = an SSINIT skill id (SETEN@0x00d21, SETCH@0x00b8c, DRAWCHP@0x01844). Column k is record k of `skill`. |
| `0xbd2b2d` | title_hp_bonuses | high | investigation | TIINIT: a bonus to max HP; SETCH and SETEN add it to the slot's G[0xc1a9ba]. |
| `0xbd32fd` | title_mp_bonuses | high | investigation | TIINIT: a bonus to max MP; SETCH and SETEN add it to the slot's G[0xc1a9f6]. |
| `0xbd3acd` | title_tp_bonuses | high | investigation | TIINIT: a bonus to max TP; SETCH and SETEN add it to the slot's G[0xc1aa32]. |
| `0xbd429d` | title_accuracy_bonuses | high | investigation | TIINIT: a bonus to 命中; SETCH and SETEN add it to the slot's G[0xc1acc6]. |
| `0xbd4a6d` | title_evasion_bonuses | high | investigation | TIINIT: a bonus to 回避; SETCH and SETEN add it to the slot's G[0xc1ace4]. |
| `0xbd523d` | title_attack_bonuses | high | investigation | TIINIT: a bonus to 攻撃力; SETCH and SETEN add it to the slot's G[0xc1aa50]. |
| `0xbd5a0d` | title_defense_bonuses | high | investigation | TIINIT: a bonus to 防御力; SETCH and SETEN add it to the slot's G[0xc1aa6e]. |
| `0xbd61dd` | title_magic_attack_bonuses | high | investigation | TIINIT: a bonus to 魔法攻撃; SETCH and SETEN add it to the slot's G[0xc1aa8c]. |
| `0xbd69ad` | title_magic_defense_bonuses | high | investigation | TIINIT: a bonus to 魔法防御; SETCH and SETEN add it to the slot's G[0xc1aaaa]. |
| `0xbd717d` | title_magic_power_bonuses | med | investigation | TIINIT: a bonus to the magic power multiplier; SETCH and SETEN add it to the slot's G[0xc1ad20]. |
| `0xbd794d` | title_attack_count_bonuses | high | investigation | TIINIT: a bonus to 攻撃回数; SETCH and SETEN add it to the slot's G[0xc1aac8]. |
| `0xbd811d` | title_defense_count_bonuses | high | investigation | TIINIT: a bonus to 防御回数; SETCH and SETEN add it to the slot's G[0xc1aae6]. |
| `0xbd88ed` | title_physical_resistance_bonuses | high | investigation | TIINIT: a bonus to 肉体抵抗; SETCH and SETEN add it to the slot's G[0xc1ab04]. |
| `0xbd90bd` | title_mental_resistance_bonuses | high | investigation | TIINIT: a bonus to 精神抵抗; SETCH and SETEN add it to the slot's G[0xc1ab22]. |
| `0xbd988d` | title_physical_speed_bonuses | high | investigation | TIINIT: a bonus to 肉体戦速; SETCH and SETEN add it to the slot's G[0xc1ab40]. |
| `0xbda05d` | title_mental_speed_bonuses | high | investigation | TIINIT: a bonus to 精神戦速; SETCH and SETEN add it to the slot's G[0xc1ab5e]. |
| `0xbda82d` | title_luck_bonuses | high | investigation | TIINIT: a bonus to 運; SETCH and SETEN add it to the slot's G[0xc1ab7c]. |
| `0xbdaffd` | title_critical_bonuses | med | investigation | TIINIT: a bonus to 痛打; SETCH and SETEN add it to the slot's G[0xc1ad02]. No record writes it. |
| `0xbdb7cd` | title_cutin_styles | med | investigation | TIINIT: how ANIME@0x005a6 animates the title's cut-in picture for a party slot; 0 plays none. |
| `0xbdbf9d` | title_battle_portraits | med | investigation | TIINIT: the portrait drawn at the screen's lower right in battle (ANIME@0x00952, DRAWCARD@0x00032). |
| `0xbdc76d` | title_cutin_pictures | high | investigation | TIINIT: the cut-in picture (ANIME@0x00620). |
| `0xbdcf3d` | title_battle_sprites | med | investigation | TIINIT: ten battle sprite sheets. DRAWCHP@0x001a9 loads column 0, or column 9 when the character's saved G[0x11cfea + 60 x character + 3] is set; BTL@0x032fe loads others. |
| `0xbe1d5d` | title_status_portraits | high | investigation | TIINIT: the status screen's portrait (DRAWCHP@0x00166). |
| `0xbe252d` | title_map_faces | high | investigation | TIINIT: the face in the map's party panel (DRAWMAPF@0x00081, CHANGETITLE@0x00362). |
| `0xbe2cfd` | title_field_sprites | med | investigation | TIINIT: the party leader's sprites on the field map, column picked by G[0x5f2073 + G[0x36df]] (CHANGETITLE@0x00051, FIELD@0x01bde). |
| `0xbef04d` | title_jump_sprites | med | investigation | TIINIT: the party leader's jump sprites, by the same column (DRAWJUMP@0x0019b). |
| `0xbfb39d` | title_carry_sprites | low | investigation | TIINIT: the party leader's sprite in CARRY@0x00750. |
| `0xbfbb6d` | title_extra_pictures | low | investigation | TIINIT: two pictures of the party leader in EXTRA1@0x002b0. |
| `0xbfcb0d` | title_voice_clips | med | investigation | TIINIT: the title's 30 voice clips, the columns as unit_voice_clips (BTL@0x08b18, ANIME@0x01fd0, RESULT@0x01612). |
| `0xc0b56d` | element_affinities | high | investigation | AFINIT: the damage percentage by defense element (row) and attack element (column); negative makes the damage negative. CALCDAMAGE@0x1d2, CALCILL@0x618. Column names: `attack_element_names`, element 0 naming column 0. |
| `0xc0b6fd` | class_icons | high | investigation | AFINIT: the class's icon, an ITINIT category whose icon DRAWCHP@0xf99 draws. |
| `0xc0b739` | class_sort_keys | med | investigation | AFINIT: the class's place in CHMODE3's class list, in steps of ten (CHMODE3@0xf9c). |
| `0xc0b775` | class_required_weapon_masks | high | investigation | AFINIT: the weapon categories an art school requires, bit k for ITINIT category k; CHECKACT@0x920 tests the unit's weapon. |
| `0xc0b7b1` | class_rank_exps | high | investigation | AFINIT: the class exp needed to pass each rank, by rank − 1; BTL@0x48d2 raises the saved rank G[0x119eb2 + 60*char + class] past it. Columns: 0=e_to_d, 1=d_to_c, 2=c_to_b, 3=b_to_a, 4=a_to_s, 5=s_to_m. |
| `0xc0b955` | class_appends | med | investigation | AFINIT: the append (2 or 3) that adds the class; CHMODE3@0x1ff7 lists it only while G[0xc781d0 + append] is set. |
| `0xc0b991` | weapon_category_flags | med | investigation | AFINIT, by ITINIT category: bit 0 takes off the G[0x11cebe] slot when equipped (CHMODE1@0x1083; inferred two-handed), bit 1 halves damage past a unit in between (CALCDAMAGE@0x8e2; inferred melee). |
| `0xc0ba07` | inn_clerk_picture | med | investigation | QUINIT sets it for the inn (CS082AA or CS081AA); INN@0xbb7 loads it into texture 0x7f. |
| `0xc0ba08` | inn_background_picture | med | investigation | QUINIT sets it for the inn (BG001A or BG029A); INN@0xba9 loads it into texture 0x7e. |
| `0xc0ba09` | skill_check_values | low | investigation | INIT2's inline array 55, 60, 57, 59, 62, 20, 14, 65, 67, 68, 69, 70. BTL@0x3914, CHECKACT@0x8fd and CHMODE3@0x180e index it with a skill id less 0xca9 and compare G[0x1218f6 + value] with G[0xc1ca18]; not decoded further. |
| `0xc1a671` | character_parties | med | investigation | CHINIT: the party the character belongs to, 0 or 1. GAMESTART@0x011e3 sets the current party G[0x363c] to it before UNITECH; BTL@0x01878 picks the battle music by it. |
| `0xc1a6d5` | character_joining_parties | high | investigation | CHINIT: the party the character joins; UNITECH@0x00302 sets the saved G[0x121892 + character] to it, or to the current party when -1. |
| `0xc1a739` | character_unit_ids | high | investigation | CHINIT: the character's EBINIT unit (UNITECH@0x00001, SETCH@0x00000, DRAWCHP@0x00ea7 and 29 more). |
| `0xc1a79d` | character_sort_keys | high | investigation | CHINIT: the order of the party lists (SORTSALLY@0x000a1, SETPTORDER@0x00000, RESULT@0x00114). |
| `0xc1a801` | character_carry_flags | med | investigation | CHINIT: a story flag (value - 1 of G[0x929]) that GAMESTART@0x011b1 sets at a new game, instead of joining the character, when the shared profile's G[0x2661db] marks it; GAMECLEAR@0x0056e reads it. |
| `0xc1a865` | character_append_numbers | med | investigation | CHINIT: the append the character comes from, written ahead of its record. READICON@0x000d1 copies the character's icon from that append's sheet. |
| `0xc259fc` | battler_condition_effects | high | investigation | A battler's condition effect, SSINIT's G[0x9f4221] for the condition (BTL@0x3e8a); SETEFFECT builds them into effect slots 6 to 35 (SETEFFECT@0xa2). |
| `0xc25a1a` | battle_animation_effects | high | investigation | BTANINIT2: the BTANINIT effect each of the animation's six slots plays, 0 for none (SETEFFECT@0x68). Slot 0 is a lead-in, slot 2 the main effect, slot 5 a closing one. |
| `0xc3447a` | battle_animation_effect_starts | high | investigation | BTANINIT2: when each slot's effect starts, in ms (ANIME@0x158a, its sound at ANIME@0x18d5). |
| `0xc42eda` | battle_animation_lengths | high | investigation | BTANINIT2: the animation's length in ms (ANIME@0x29a); a voice is scheduled against it (ANIME@0x2143). |
| `0xc455ea` | battle_effect_pictures | high | investigation | BTANINIT: the slot's effect picture (MVB<effect id>.AGF for 423 of 439 effects); ANIME plays it as a movie (ANIME@0x1642) or sets it as a sheet (ANIME@0x1602). |
| `0xc4560e` | battle_effect_still_kinds | med | investigation | BTANINIT: 0 for a movie, nonzero for a still picture set as a texture, with colour key 0xff00 for 1 (ANIME@0x15ca); only effect 3 has one. |
| `0xc45632` | battle_effect_blend_modes | high | investigation | BTANINIT: the blend mode gfx-draw-color gets: 1 additive glow, 3 the subtractive path (ANIME@0x22a1). |
| `0xc45656` | battle_effect_widths | high | investigation | BTANINIT: the effect surface's width (ANIME@0x1632); the effect is drawn centred on x (ANIME@0x221f). |
| `0xc4567a` | battle_effect_heights | high | investigation | BTANINIT: the effect surface's height; the effect is drawn with its foot on y (ANIME@0x2234). |
| `0xc4569e` | battle_effect_sheet_columns | high | investigation | BTANINIT: a still sheet's columns, passed to animate-gfx-srcrect-target (ANIME@0x22c6). |
| `0xc456c2` | battle_effect_sheet_rows | low | investigation | BTANINIT: not read; a still sheet's rows, inferred from effect 3's 5 x 5 sheet of 24 frames. |
| `0xc456e6` | battle_effect_frame_counts | high | investigation | BTANINIT: a still sheet's frame count (ANIME@0x22bf). |
| `0xc4570a` | battle_effect_lengths | high | investigation | BTANINIT: the effect's length in ms; for a movie ANIME writes the movie's own length into it (ANIME@0x1652). |
| `0xc4572e` | battle_effect_placed_flags | med | investigation | BTANINIT: 1 places the effect from a battler's cell (the target's, inferred), 0 at the point (512, 576) (ANIME@0x215d); every effect has 1. |
| `0xc45752` | battle_effect_x_offsets | high | investigation | BTANINIT: an x offset of the effect (ANIME@0x2247). |
| `0xc45776` | battle_effect_y_offsets | high | investigation | BTANINIT: a y offset of the effect (ANIME@0x225a). |
| `0xc4579a` | battle_effect_sounds | high | investigation | BTANINIT: a sound effect for the slot, played at the slot's start plus battle_effect_sound_delays (ANIME@0x18ad); no layer writes one. |
| `0xc457be` | battle_effect_sound_delays | high | investigation | BTANINIT: the slot sound's delay in ms (ANIME@0x18e7); no layer writes one. |
| `0xc457e2` | battle_effect_cue_times | low | investigation | BTANINIT: up to three moments, in ms after the slot starts, at which ANIME sets a value falling from 5 over 100 ms (ANIME@0x1afc); what it moves is not read. |
| `0xc4584e` | objective_sort_keys | high | investigation | FAINIT, written first in each record: INFO1@0x812 sorts ids 0 to 199 by it. |
| `0xc4601e` | objective_required_flags | high | investigation | FAINIT: story flags, each a flag + 1, that must all be set for INFO1 to list the objective (INFO1@0x8a1). |
| `0xc4778e` | objective_done_flags | high | investigation | FAINIT, on 9 objectives: story flags, each a flag + 1, none of which may be set for INFO1 to list it (INFO1@0x8fb). |
| `0xc48eff` | profile_unit_ids | high | investigation | CIINIT: the speaker whose having spoken (G[0x2625cb]) shows the profile (INFO3@0x00a0c); the table's leader. |
| `0xc48fc7` | profile_scenes | med | investigation | CIINIT: up to three scenes, each scene + 1; the profile is listed once one is seen in G[0x25febb], or always with none (INFO3@0x00973). |
| `0xc4921f` | profile_pictures | high | investigation | CIINIT: the profile's picture (INFO3@0x00fcd). |
| `0xc492e7` | profile_picture_xs | high | investigation | CIINIT: the picture's x offset (INFO3@0x0100c). |
| `0xc493af` | profile_picture_ys | high | investigation | CIINIT: the picture's y offset (INFO3@0x0101f). |
| `0xc49478` | glossary_unlock_scenes | high | investigation | VIINIT: scenes, each a scene id + 1, any of which unlocks the entry once seen (scene_seen_flags, INFO2@0xa0c to 0xa56); an entry whose first is 0 is always listed. |
| `0xc4a030` | glossary_pictures | high | investigation | VIINIT: the entry's picture, VI000 to VI081 (INFO2@0xfe1). |
| `0xc4a418` | glossary_picture_x_offsets | high | investigation | VIINIT, 50 on every entry: added to the picture's centred x (INFO2@0x1027). |
| `0xc4a800` | glossary_picture_y_offsets | high | investigation | VIINIT, 33 on 50 entries: added to the picture's y (INFO2@0x103a). |
| `0xc4b0e3` | direction_x_steps | high | investigation | INIT2's inline array -1, 0, 1, 0: a direction's x step, read by ATSEEK, MVSEEK and RTN_M031 with direction_y_steps. |
| `0xc4b0e7` | direction_y_steps | high | investigation | INIT2's inline array 0, -1, 0, 1: a direction's y step, beside direction_x_steps. |
| `0xc4b25d` | terrain_move_costs | high | investigation | SOINIT: the move cost of the terrain for each move type (troop_move_types); 0 cannot enter (MVSEEK@0x204). Columns: 0=foot, 1=cavalry, 2=water_dragon, 3=flying. |
| `0xc4b325` | troop_sheets | high | investigation | TPINIT: the troop's map sheet for side 1 (TP001A.AGF) and side 2 (TP001B.AGF), SLG@0x2116. Columns: 0=side_1, 1=side_2. |
| `0xc4b361` | troop_move_types | high | investigation | TPINIT: the move type, a column of terrain_move_costs (MVSEEK@0x4c). Values: 0 Foot, 1 Cavalry, 2 Water dragon, 3 Flying. |
| `0xc4b37f` | troop_stats | high | investigation | TPINIT: move (range = value + 1, MVSEEK@0x33), attack, defence and magic; SLG@0x428 copies them, plus the rank's bonus, into the saved unit state. Columns: 0=move, 1=attack, 2=defence, 3=magic. |
| `0xc4b3f7` | troop_tactics | high | investigation | TPINIT: the troop's own tactic, a RUINIT id (SLG@0x48e). |
| `0xc4b415` | troop_soldiers | high | investigation | TPINIT: the troop's soldiers, copied into the saved G[0x263044] and G[0x263062] (SLG@0x3cd). |
| `0xc4b433` | troop_morale | high | investigation | TPINIT: the starting morale, copied into the saved G[0x263080] (SLG@0x3f3); CALCRUSH multiplies attack by (100 + morale) / 100. |
| `0xc4b451` | troop_rout_points | med | investigation | TPINIT: the war-gauge points the troop's rout is worth when its morale falls below 0 (SLG@0x1aa1). |
| `0xc4b46f` | commander_leaderships | high | investigation | CAINIT: leadership, which divides the unit's morale loss (SLG@0x19e1). |
| `0xc4b537` | commander_attacks | high | investigation | CAINIT: the attack of the commander's tactics (CALCRUSH@0x78). |
| `0xc4b5ff` | commander_tactics | high | investigation | CAINIT: the commander's tactic, a RUINIT id, copied into the saved unit state (SLG@0x4eb). |
| `0xc4b6c7` | commander_duel_battles | high | investigation | CAINIT: the 一騎討ち battle SLG@0x15fe enters BTL with (into G[0x276e47]); only commander 101 has one. |
| `0xc4b78f` | commander_sheets | high | investigation | CAINIT: column 0 the commander's map sprite (CPS001A.AGF, SLG@0x20dc), column 1 a larger sheet (CPS001B.AGF, SLG@0x3958). Columns: 0=map, 1=large. |
| `0xc4b91f` | tactic_kinds | high | investigation | RUINIT: 0 a troop's basic attack, 1 a commander's attack, 2 magic (uses the magic stat), 3 a duel, which CALCRUSH@0x21 does not compute. Values: 0 Basic, 1 Commander, 2 Magic, 3 Duel. |
| `0xc4b983` | tactic_min_ranges | high | investigation | RUINIT: the least range (CALCSCOPE; drawn at SLG@0x3441). |
| `0xc4b9e7` | tactic_max_ranges | high | investigation | RUINIT: the greatest range (CALCSCOPE; SLG@0x345e). |
| `0xc4ba4b` | tactic_powers | high | investigation | RUINIT: the power multiplier (CALCRUSH@0xbf). |
| `0xc4baaf` | tactic_cooldowns | high | investigation | RUINIT: 1 always usable, n unusable for n - 1 turns after use (SELACT@0xb20, SLG@0x1891). |
| `0xc4bb13` | tactic_blend_modes | high | investigation | RUINIT: the effect's blend mode, 1 additive or 3 subtractive (SLG@0x3b85). Values: 1 Additive, 3 Subtractive. |
| `0xc4bb77` | tactic_animations | high | investigation | RUINIT: the effect's animation (MVR001.AGF ...), played as a movie (SLG@0x3b5d). |
| `0xc4bbdb` | map_routine_step_kinds | high | investigation | SRINIT: each step's rule, 1 to 62 for RTN_M001 to RTN_M062; 0 ends the routine and a rule with no script is skipped (MVRTN@0xc6). |
| `0xc509fb` | map_routine_step_chances | high | investigation | SRINIT: the percent chance the step runs (MVRTN@0x17d). |
| `0xc5581b` | map_routine_step_args | high | investigation | SRINIT: the rule's first argument: a switch G[0x2630f8 + arg] for rules 2 and 3, a cell's x for 11 and 12, a map unit for 13 (RTN_M002@0x0, RTN_M011@0x0, RTN_M013@0x0). |
| `0xc5a63b` | map_routine_step_args_2 | high | investigation | SRINIT: the second argument: a cell's y for rules 11 and 12 (RTN_M011@0x10). |
| `0xc6debb` | map_routine_step_flags | high | investigation | SRINIT: a story flag the step needs, G[0x929 + flag - 1] (MVRTN@0xe7); no routine sets one. |
| `0xc72cdb` | map_routine_step_limits | high | investigation | SRINIT: how many times the step may run, against the saved count G[0x265a08] (MVRTN@0x12f). |
| `0xc77afb` | rank_points | high | investigation | GRINIT: the war points a rank needs; SLG@0x231 picks the highest rank at most war_points. |
| `0xc77b05` | rank_bonuses | high | investigation | GRINIT: bonuses to the four numbers of the player's units (SLG@0x463). Columns: 0=move, 1=attack, 2=defence, 3=magic. |
| `0xc77b38` | fishing_rod_stats | high | investigation | EX1INIT: rod k (item 46 + k): the nearest and farthest cast point it reaches, and the speed of the cast marker (EXTRA1@0x06ce5, 0x06bb5). Columns: 0=nearest_point, 1=farthest_point, 2=marker_speed. |
| `0xc77b44` | fishing_bait_hit_rows | high | investigation | EX1INIT: bait k (item 601 + k): the row of fishing_hit_weights for each cast point 1 to 5 (EXTRA1@0x06d5b). |
| `0xc77b6c` | fishing_hit_weights | high | investigation | EX1INIT (Ex1HitProbTbl, EXTRA1@0x06dc2): the bite weight of each fish for a bait's row; the bite draws a fish weighted by it times fishing_spot_fish (EXTRA1@0x06d8d). |
| `0xc77dc4` | fish_stats | med | investigation | EX1INIT: fish k (item 611 + k): the points a catch gives (EXTRA1@0x02048), the points an escape takes (EXTRA1@0x0201b), and the x and width of its catch zone on the reel gauge (EXTRA1@0x03051, 0x03083). Columns: 0=catch_points, 1=escape_points, 2=zone_x, 3=zone_width. |
| `0xc77e00` | fishing_rank_points | high | investigation | EX1INIT: the fishing points each rank needs, 0 to 300 (EXTRA1@0x07553). |
| `0xc77e07` | fish_element_values | low | investigation | EX1INIT: 400 for every fish; EXTRA1 writes it into a cell of a picture element (EXTRA1@0x049b2). Not read further. |
| `0xc77e16` | fishing_spot_fish | high | investigation | EX1INIT (Ex1FishExist): 1 where fish k lives at the spot; it multiplies the bite weights (EXTRA1@0x06d8d). |
| `0xc77f7e` | fishing_spot_music | high | investigation | EX1INIT: the music EXTRA1 plays at the spot (EXTRA1@0x0081c). |
| `0xc77f96` | fishing_spot_sounds | high | investigation | EX1INIT: a looping sound of the spot (A0628.WAV or B0503B.WAV), 0 for none (EXTRA1@0x00828). |
| `0xc77fae` | fishing_spot_backgrounds | high | investigation | EX1INIT: the spot's background picture, SO027E and the rest (EXTRA1@0x00885). |
| `0xc77fc6` | fishing_spot_overlays | med | investigation | EX1INIT: a second picture drawn over the spot's background, 0 for none (EXTRA1@0x0089a). |
| `0xc77fde` | fishing_spot_fisher_poses | med | investigation | EX1INIT: 0 or 1, the column of the fisher's pictures G[0xbfbb6d + 2 * G[0x363b]] the spot shows (EXTRA1@0x0029b). |
| `0xc77ff6` | fishing_spot_cast_ranges | low | investigation | EX1INIT: six ranges on the cast gauge, a start and a width each, -1 for none; drawn at EXTRA1@0x037a0, and a cast that stops in one ends in state 2 (EXTRA1@0x01ada), whose meaning is not read. |
| `0xc78116` | fishing_spot_maps | high | investigation | EX1INIT (AllMapNo): the map of the spot; EXTRA1 picks the spot whose map is G[0x363f] (EXTRA1@0x00256). |
| `0xc78130` | cg_thumbnail_sheet_chain | high | investigation | Pictures: slot n is append n's CG thumbnail sheet, $n$SO021G.AGF; slot 0 is empty. CGMODE@0x1198 loads slot n into texture 0x5e + n, which cg_append_numbers selects. |
| `0xc78150` | replay_thumbnail_sheet_chain | high | investigation | Pictures: slot n is append n's replay thumbnail sheet, $n$SO022F.AGF; slot 0 is empty. HMODE@0xd29 loads slot n into texture 0x5e + n, which replay_append_numbers selects. |
| `0xc78170` | character_icon_sheet_chains | med | investigation | Pictures, three rows (SO034, SO015, SO038 and each append's). READICON composes the row its caller puts in chain_slot_index (0 ALCHEMY, CHMODE, SHOP, TOWN; 1 DIVIDECH, SELCH; 2 RESULT) into texture G[0x268b3c], taking each character 1 to 99 whose G[0xc1a865 + id] is above 0 from that layer's sheet. |
| `0xc781d0` | chmode3_sheet_chain | med | investigation | Pictures: SO039 and each append's (slot 2 is 0x20000d6). CHMODE3@0x1f62 walks it. |
| `0xc781f0` | battle_sheet_chain | med | investigation | Pictures: SO036 and $1$SO036. BTL@0x9148 loads one per layer into texture 0xac + layer, and BTL@0x91e4 picks the layer by G[0x9f31b9 + i]. |
| `0xc78210` | ssmes_chain_script_ids | high | investigation | SSMES and each append's $n$SSMES; DRAWSSMES walks it until chain_handled is set. |
| `0xc78230` | itmes_chain_script_ids | high | investigation | ITMES and each append's $n$ITMES; DRAWITMES walks it until chain_handled is set. |
| `0xc78250` | times_chain_script_ids | high | investigation | TIMES and each append's $n$TIMES; DRAWTIMES walks it until chain_handled is set. |
| `0xc78270` | vimes_chain_script_ids | high | investigation | VIMES and each append's $n$VIMES: the glossary's text. INFO2@0x10e6 walks it with glossary_selected_id set, until chain_handled is set. |
| `0xc78290` | cimes_chain_script_ids | high | investigation | CIMES and each append's $n$CIMES; INFO3@0x10b0 walks it until chain_handled is set. |
| `0xc782b0` | aminit_chain_script_ids | high | investigation | AMINIT and each append's $n$AMINIT. ALLMAP@0x254 walks it on each map pass, and ALLMAP@0x30 when a scene's SHOWALLMAP enters the map. |
| `0xc782d0` | siinit_chain_script_ids | high | investigation | SIINIT and each append's $n$SIINIT. TOWN@0x1af walks it after the START chain and SCJUMP's context 2. |
| `0xc782f0` | shinit_chain_script_ids | high | investigation | SHINIT and each append's $n$SHINIT. SHOP@0x1f51 walks it. |
| `0xc78310` | quinit_chain_script_ids | high | investigation | QUINIT and each append's $n$QUINIT. INN@0x8a walks it on entry, and INIT2@0x24e on a load where in_inn is set. |
| `0xc78330` | rminit_chain_script_ids | high | investigation | RMINIT and each append's $n$RMINIT. FIELD@0x92 walks it on entry, and INIT2@0x1e5 on a load where on_dungeon_map is set. |
| `0xc78350` | eninit_chain_script_ids | high | investigation | ENINIT and each append's $n$ENINIT. FIELD@0x63a walks it, and MP01B0101@0x50b, MP13B0101@0x2ab, MP13B0404@0x355 and MPEV01B0203@0x159f. |
| `0xc78370` | esinit_chain_script_ids | high | investigation | ESINIT and each append's $n$ESINIT. SETENP@0x414 walks it; BTL calls SETENP. |
| `0xc78390` | csinit_chain_script_ids | high | investigation | CSINIT and each append's $n$CSINIT. BTL@0xcf walks it. |
| `0xc783b0` | pzinit_chain_script_ids | high | investigation | PZINIT and each append's $n$PZINIT. SETMAPDATA@0x113 walks it until chain_handled is set; FIELD and INIT2 call SETMAPDATA. |
| `0xc783d0` | stinit_chain_script_ids | high | investigation | STINIT and each append's $n$STINIT. SLG@0x2df walks it. |
| `0xc78410` | btaninit_chain_script_ids | high | investigation | BTANINIT and each append's $n$BTANINIT. SETEFFECT@0xf0 walks it; ANIME calls SETEFFECT. |
| `0xc78430` | scjump_chain_script_ids | high | investigation | SCJUMP and each append's $n$SCJUMP ($3$SCJUMP is an empty stub). Walked while scjump_chain_pending is 1, in the context scjump_context_id, by ALLMAP, FIELD, TOWN, INN, MENU and BTL (docs/verita-data-tables.md, Scene dispatch). |
| `0xc78450` | fight_phase_chain_script_ids | high | investigation | Three rows, base only: ROUND, BEFOREFIGHT, AFTERFIGHT. SLG@0x1667 walks the row in its local 0x4dd. |
| `0xc784b0` | map_entry_chain_script_ids | high | investigation | Two rows: CLOSE (row 0, walked by ALLMAP@0x112) and START (row 1, walked by FIELD@0x225 and TOWN@0x105), with each append's. |
| `0xc784f0` | checkss_chain_script_ids | high | investigation | Four rows: CHECKSS1 to CHECKSS4, and $3$CHECKSS2 in row 1. CHECKSS@0x249 walks rows 0 to 2 and CHECKSS@0x548 row 3; DEBUGADV and scenes call CHECKSS. |
| `0xc78570` | ss_damage_chain_script_ids | high | investigation | Two rows: SETSSBONUS (with $3$SETSSBONUS, walked by CALCSS@0x4c) and SETSSDAMAGE (walked by COMMITSS@0x177); CALCDAMAGE calls both. |
| `0x4` | — | low | auto-shape | array |
| `GS 0xd2` | — | low | auto-shape | array |
| `GS 0x1a7` | — | low | auto-shape | array |
| `GS 0x1ab` | — | low | auto-shape | array |
| `GS 0x1157` | — | low | auto-shape | array |
| `GS 0x116b` | — | low | auto-shape | array |
| `GS 0x1234` | — | low | auto-shape | array |
| `GS 0x123e` | — | low | auto-shape | array |
| `0x34e3` | — | low | auto-shape | array |
| `0x351f` | — | low | auto-shape | array |
| `0x3533` | — | low | auto-shape | array |
| `0x3547` | — | low | auto-shape | array |
| `0x36f3` | — | low | auto-shape | array |
| `0x10c153` | — | low | auto-shape | array |
| `0x118ef4` | — | low | auto-shape | array |
| `0x118efd` | — | low | auto-shape | array |
| `0x118f00` | — | low | auto-shape | array |
| `0x118f03` | — | low | auto-shape | array |
| `0x118f06` | — | low | auto-shape | array |
| `0x118f09` | — | low | auto-shape | array |
| `0x118f0d` | — | low | auto-shape | array |
| `0x118fd8` | — | low | auto-shape | array |
| `0x11967e` | — | low | auto-shape | array |
| `0x1197aa` | — | low | auto-shape | array |
| `0x11980e` | — | low | auto-shape | array |
| `0x119872` | — | low | auto-shape | array |
| `0x1198d6` | — | low | auto-shape | array |
| `0x11993a` | — | low | auto-shape | array |
| `0x11999e` | — | low | auto-shape | array |
| `0x119a02` | — | low | auto-shape | array |
| `0x119a66` | — | low | auto-shape | array |
| `0x119aca` | — | low | auto-shape | array |
| `0x119b2e` | — | low | auto-shape | array |
| `0x119b92` | — | low | auto-shape | array |
| `0x119bf6` | — | low | auto-shape | array |
| `0x119c5a` | — | low | auto-shape | array |
| `0x119cbe` | — | low | auto-shape | array |
| `0x119d22` | — | low | auto-shape | array |
| `0x119d86` | — | low | auto-shape | array |
| `0x119dea` | — | low | auto-shape | array |
| `0x119e4e` | — | low | auto-shape | array |
| `0x11cd92` | — | low | auto-shape | array |
| `0x11cdf6` | — | low | auto-shape | array |
| `0x11ce5a` | — | low | auto-shape | array |
| `0x11cebe` | — | low | auto-shape | array |
| `0x11cf22` | — | low | auto-shape | array |
| `0x11cf86` | — | low | auto-shape | array |
| `0x12163a` | — | low | auto-shape | array |
| `0x121702` | — | low | auto-shape | array |
| `0x121766` | — | low | auto-shape | array |
| `0x1217ca` | — | low | auto-shape | array |
| `0x12182e` | — | low | auto-shape | array |
| `0x121892` | — | low | auto-shape | array |
| `0x16af92` | — | low | auto-shape | array |
| `0x2625cb` | — | low | auto-shape | array |
| `0x2629b3` | — | low | auto-shape | array |
| `0x262d9b` | — | low | auto-shape | array |
| `0x262da0` | — | low | auto-shape | array |
| `0x262e6d` | — | low | auto-shape | array |
| `0x262e72` | — | low | auto-shape | array |
| `0x262e77` | — | low | auto-shape | array |
| `0x262f32` | — | low | auto-shape | array |
| `0x262fea` | — | low | auto-shape | array |
| `0x263008` | — | low | auto-shape | array |
| `0x263026` | — | low | auto-shape | array |
| `0x263044` | — | low | auto-shape | array |
| `0x263062` | — | low | auto-shape | array |
| `0x263080` | — | low | auto-shape | array |
| `0x26309e` | — | low | auto-shape | array |
| `0x2630bc` | — | low | auto-shape | array |
| `0x2630da` | — | low | auto-shape | array |
| `0x2630f8` | — | low | auto-shape | array |
| `0x265c60` | — | low | auto-shape | array |
| `0x265cc4` | — | low | auto-shape | array |
| `0x265d28` | — | low | auto-shape | array |
| `0x265d8c` | — | low | auto-shape | array |
| `0x265df0` | — | low | auto-shape | array |
| `0x265e54` | — | low | auto-shape | array |
| `0x265eb8` | — | low | auto-shape | array |
| `0x265f1c` | — | low | auto-shape | array |
| `0x265f80` | — | low | auto-shape | array |
| `0x265fe4` | — | low | auto-shape | array |
| `0x266048` | — | low | auto-shape | array |
| `0x2660ac` | — | low | auto-shape | array |
| `0x266110` | — | low | auto-shape | array |
| `0x266174` | — | low | auto-shape | array |
| `0x2661d8` | — | low | auto-shape | array |
| `0x2661db` | — | low | auto-shape | array |
| `0x266374` | — | low | auto-shape | array |
| `0x2663fd` | — | low | auto-shape | array |
| `0x266403` | — | low | auto-shape | array |
| `0x268b69` | — | low | auto-shape | array |
| `0x27539d` | — | low | auto-shape | array |
| `0x2753b2` | — | low | auto-shape | array |
| `0x27547a` | — | low | auto-shape | array |
| `0x275548` | — | low | auto-shape | array |
| `0x275553` | — | low | auto-shape | array |
| `0x2755b7` | — | low | auto-shape | array |
| `0x27561f` | — | low | auto-shape | array |
| `0x275685` | — | low | auto-shape | array |
| `0x2756a3` | — | low | auto-shape | array |
| `0x2756c1` | — | low | auto-shape | array |
| `0x276e47` | — | low | auto-shape | array |
| `0x276e4a` | — | low | auto-shape | array |
| `0x276e56` | — | low | auto-shape | array |
| `0x276e74` | — | low | auto-shape | array |
| `0x276e9a` | — | low | auto-shape | array |
| `0x27b550` | — | low | auto-shape | array |
| `0x36f9eb` | — | low | auto-shape | array |
| `0x36f9f8` | — | low | auto-shape | array |
| `0x36f9fd` | — | low | auto-shape | array |
| `0x5dd8fc` | — | low | auto-shape | array |
| `0x5dd924` | — | low | auto-shape | array |
| `0x5dd94c` | — | low | auto-shape | array |
| `0x5dd9b0` | — | low | auto-shape | array |
| `0x5dd9b8` | — | low | auto-shape | array |
| `0x5dd9bd` | — | low | auto-shape | array |
| `0x5dda76` | — | low | auto-shape | array |
| `0x5ddaa1` | — | low | auto-shape | array |
| `0x5ddac9` | — | low | auto-shape | array |
| `0x5ddaf1` | — | low | auto-shape | array |
| `0x5ddb19` | — | low | auto-shape | array |
| `0x5ddb41` | — | low | auto-shape | array |
| `0x5ddb69` | — | low | auto-shape | array |
| `0x5ddb91` | — | low | auto-shape | array |
| `0x5ddbb9` | — | low | auto-shape | array |
| `0x5ddbe1` | — | low | auto-shape | array |
| `0x5ddc09` | — | low | auto-shape | array |
| `0x5ddc31` | — | low | auto-shape | array |
| `0x5ddc59` | — | low | auto-shape | array |
| `0x5ddc81` | — | low | auto-shape | array |
| `0x5ddca9` | — | low | auto-shape | array |
| `0x5ddcd1` | — | low | auto-shape | array |
| `0x5ddcf9` | — | low | auto-shape | array |
| `0x5e7a01` | — | low | auto-shape | array |
| `0x5e7ac9` | — | low | auto-shape | array |
| `0x5eca79` | — | low | auto-shape | array |
| `0x5ecadd` | — | low | auto-shape | array |
| `0x5ecb41` | — | low | auto-shape | array |
| `0x5ecba5` | — | low | auto-shape | array |
| `0x5ecc09` | — | low | auto-shape | array |
| `0x5ecc6d` | — | low | auto-shape | array |
| `0x5ed0b9` | — | low | auto-shape | array |
| `0x5ed11d` | — | low | auto-shape | array |
| `0x9d17a9` | — | low | auto-shape | array |
| `0xc0ba18` | — | low | auto-shape | array |
| `0xc0ba1d` | — | low | auto-shape | array |
| `0xc0ba81` | — | low | auto-shape | array |
| `0xc0bae5` | — | low | auto-shape | array |
| `0xc0bb49` | — | low | auto-shape | array |
| `0xc0bbad` | — | low | auto-shape | array |
| `0xc1a8ca` | — | low | auto-shape | array |
| `0xc1a906` | — | low | auto-shape | array |
| `0xc1a924` | — | low | auto-shape | array |
| `0xc1a942` | — | low | auto-shape | array |
| `0xc1a960` | — | low | auto-shape | array |
| `0xc1a97e` | — | low | auto-shape | array |
| `0xc1a99c` | — | low | auto-shape | array |
| `0xc1a9ba` | — | low | auto-shape | array |
| `0xc1a9d8` | — | low | auto-shape | array |
| `0xc1a9f6` | — | low | auto-shape | array |
| `0xc1aa14` | — | low | auto-shape | array |
| `0xc1aa32` | — | low | auto-shape | array |
| `0xc1aa50` | — | low | auto-shape | array |
| `0xc1aa6e` | — | low | auto-shape | array |
| `0xc1aa8c` | — | low | auto-shape | array |
| `0xc1aaaa` | — | low | auto-shape | array |
| `0xc1aac8` | — | low | auto-shape | array |
| `0xc1aae6` | — | low | auto-shape | array |
| `0xc1ab04` | — | low | auto-shape | array |
| `0xc1ab22` | — | low | auto-shape | array |
| `0xc1ab40` | — | low | auto-shape | array |
| `0xc1ab5e` | — | low | auto-shape | array |
| `0xc1ab7c` | — | low | auto-shape | array |
| `0xc1ab9a` | — | low | auto-shape | array |
| `0xc1abb8` | — | low | auto-shape | array |
| `0xc1abd6` | — | low | auto-shape | array |
| `0xc1abf4` | — | low | auto-shape | array |
| `0xc1ac12` | — | low | auto-shape | array |
| `0xc1ac30` | — | low | auto-shape | array |
| `0xc1ac4e` | — | low | auto-shape | array |
| `0xc1ac6c` | — | low | auto-shape | array |
| `0xc1ac8a` | — | low | auto-shape | array |
| `0xc1aca8` | — | low | auto-shape | array |
| `0xc1acc6` | — | low | auto-shape | array |
| `0xc1ace4` | — | low | auto-shape | array |
| `0xc1ad02` | — | low | auto-shape | array |
| `0xc1ad20` | — | low | auto-shape | array |
| `0xc1ad3e` | — | low | auto-shape | array |
| `0xc1ad5c` | — | low | auto-shape | array |
| `0xc1ad7a` | — | low | auto-shape | array |
| `0xc1c2ce` | — | low | auto-shape | array |
| `0xc1c2ec` | — | low | auto-shape | array |
| `0xc1c30a` | — | low | auto-shape | array |
| `0xc1c328` | — | low | auto-shape | array |
| `0xc1c346` | — | low | auto-shape | array |
| `0xc1c364` | — | low | auto-shape | array |
| `0xc1c382` | — | low | auto-shape | array |
| `0xc1c3a0` | — | low | auto-shape | array |
| `0xc1c3be` | — | low | auto-shape | array |
| `0xc1c3dc` | — | low | auto-shape | array |
| `0xc1c3fa` | — | low | auto-shape | array |
| `0xc1c418` | — | low | auto-shape | array |
| `0xc1ca12` | — | low | auto-shape | array |
| `0xc1ca30` | — | low | auto-shape | array |
| `0xc256ee` | — | low | auto-shape | array |
| `0xc25709` | — | low | auto-shape | array |
| `0xc25966` | — | low | auto-shape | array |
| `0xc4b0d7` | — | low | auto-shape | array |
| `0xc4b14e` | — | low | auto-shape | array |
| `0xc4b158` | — | low | auto-shape | array |
| `0xc4b162` | — | low | auto-shape | array |
| `0xc4b16c` | — | low | auto-shape | array |
| `0xc4b18b` | — | low | auto-shape | array |
| `0xc4b1a9` | — | low | auto-shape | array |
| `0xc4b1c7` | — | low | auto-shape | array |
| `0xc4b1e5` | — | low | auto-shape | array |
| `0xc4b203` | — | low | auto-shape | array |
| `0xc4b221` | — | low | auto-shape | array |
| `0xc4b23f` | — | low | auto-shape | array |

## index-pointer

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x0` | system_mode | high | investigation | The mode SYSTEM4 runs each pass: 1 a scene, 2 ALLMAP, 3 FIELD, 4 BTL, 5 TOWN, 6 ARENA, 7 INN, 8 SLG, 9 extra_game_scripts[extra_game_id] (SYSTEM4@0x719). A SCJUMP rule that takes the turn sets 1. |
| `0x90f` | current_scene_script_id | med | investigation | SYSTEM4@0x366 copies scene_script_ids[current_scene_id] into it before it runs the scene (default 0x5e), so a save keeps the playing scene's script. |
| `0x3489` | adv_speaker_id | high | investigation | The page's speaker: an index into CNINIT's names, or -1 for narration. Every spoken page sets it before show_speaker, which draws the name plate from it; clear_speaker sets -1. |
| `0x348b` | scjump_context_id | high | investigation | Which of SCJUMP's rule lists runs. 1 ALLMAP@0x181 each map pass; 2 FIELD@0x256 or TOWN@0x128 on entry; 3 TOWN@0x20b for each open location (rules give the location a scene); 5 BTL@0xa2b1 after a battle; 6 MENU@0x648; 7 INN@0x76b when a quest is taken; 8 INN@0xa5 on entry (rules queue quest scenes); 9 FIELD@0x55f back from a random battle (the scene plays over the map). 4 is never set. |
| `0x363c` | current_party_index | med | investigation | The party in play, 0 to 2. CHECKCONTRACT and INN mark a quest with bit 17 + it, QUESTCLEAR pays into the party's money G[0x118f0d + it], and SCJUMP rules test it. |
| `0x363f` | current_place_id | high | investigation | The place in play: an AMINIT2 area, the MPINIT map of a dungeon area and the SIINIT town branch of a town area. FIELD and TOWN read their tables by it, and SCJUMP's rules test it 334 times. |
| `0x36a4` | world_map_id | high | investigation | The world map in play, 1 or 2; ALLMAP@0x22b sets it from area_worlds and AMINIT branches on it. |
| `0x36b0` | town_site_id | med | investigation | The town site in play (TOWN@0x1d9): a location 1 to 29, for which TOWN@0x20b runs SCJUMP's context 3; that context's rules test it. |
| `0x36cf` | arena_match_id | high | investigation | The arena match chosen in ARENA; ARINIT's arrays are read by it (ARENA@0xaf7, SETEN@0xaf, RESULT@0x3a1). |
| `0x36d1` | map_block_row | high | investigation | Saved: the row of the block in play; RMINIT's arrays are read at 30 * row + block (FIELD@0x60d). |
| `0x36d2` | map_block_column | high | investigation | Saved: the block in play within its row (FIELD@0x60d). |
| `0x36df` | field_land_id | high | investigation | The land type under the player: FIELD@0x1b60 writes AGERC _GetLand's answer here. LAINIT's arrays are read by it. |
| `0x36ea` | map_music_variant | high | investigation | Saved: which of the block's three musics plays (FIELD@0x668); map scripts set it (MP27F0101@0x78f). |
| `0x36eb` | map_encounter_variant | high | investigation | Saved: which of the block's two encounter sets is in play (FIELD@0x606); map scripts set it (MP01B0101@0x4cb). |
| `0x118ecf` | current_inn_id | med | investigation | The inn, 1 to 9: QUINIT branches on it for the inn's own quests (QUINIT@0x5e4 ...), and a quest id of 100 or more is its slot in this inn's row of quest_states. INN@0x766 sets it to the quest's row while SCJUMP's context 7 runs. |
| `0x118ef7` | party_area_values | low | investigation | Beside party_area_ids: INIT sets 1, FIELD@0x1f7 sets [2] to G[0x42a588 + current_area_id], and ALLMAP@0x22b reads the party's into G[0x36a4]. What it holds is not decoded. |
| `0x118efa` | party_area_ids | med | investigation | A party's area, an AMINIT2 id: INIT sets 1 for party 0 and 2 for party 1, and FIELD@0x1f7 sets [2] to current_area_id. |
| `0x1218f6` | character_titles | high | investigation | Saved: the character's current TIINIT title. UNITECH@0x0023b sets it to 20 x character on joining; CHANGETITLE@0x00001 to an earned title. |
| `0x262e7d` | war_stage_id | high | investigation | The war stage, 1 to 5, set by the scene that starts SLG (SC2400, SC1350, SC5000, SC2350, DEBUGSLG@0x2c); STINIT branches on it. |
| `0x26636d` | fishing_rod_in_use | high | investigation | Saved: the rod in use (0 to 3); EXTRA1 loads it on entry and stores it on leaving (EXTRA1@0x0033f, 0x07c3e). |
| `0x266384` | selected_member_index | low | investigation | A saved index into G[0x11967e] that ALCHEMY, SHOP, CHMODE1 and CHMODE4 read and PUTHAI sets; INIT and INIT2@0x169 set 1 where it is 0. What it selects is not decoded. |
| `0x268b36` | chain_slot_index | med | investigation | The slot most chain walkers go through, 0 to 31 (MENU@0x652, INN@0x775). READICON's callers set it to the icon chain's row instead. |
| `0x269b2c` | shared_number | high | investigation | A number scripts hand each other or use as scratch (547 scripts: loop indexes, picture queries, amounts). ADDEXP reads it as the experience to add, which its callers set just before calling it (RESULT@0x9a9 and 16 more). |
| `0x370816` | enemy_setup_unit | high | investigation | The unit SETEN builds an enemy slot from; BTL@0x039a6 and SETENP@0x0055a set it before SETEN runs (SETEN@0x00000). |
| `0x42ab0d` | shop_id | high | investigation | The shop SHOP opens (1 to 23): a scene script sets it before calling SHOP (SC0600@0x00d6a), SHINIT branches on it, and SHOP clears it on leaving (SHOP@0x04310). |
| `0x5d852c` | selected_quest_slot | med | investigation | The quest the player chose on INN's board, a slot of its row (INN@0x6ed); SCJUMP's context 7 rules test it. |
| `0x5d852d` | current_quest_id | high | investigation | The quest CHECKCONTRACT, QUESTCLEAR and INN's board work on, an id of QUINIT (0 to 199). SCJUMP's context 8 rules and SYSTEM4@0x631 set it. |
| `0x5dda74` | character_arg_id | high | investigation | The character the shared character scripts work on (ADDEXP, CALCREVISE, CALCLVUP, CHECKSKILL and 153 more read it); a caller sets it before running them, and INIT2 and RESULT loop it over the characters. |
| `0x5f2005` | field_puzzle_id | high | investigation | The box puzzle of the section in play, 0 for none; SETMAPDATA@0x113 runs PZINIT for it. |
| `0x6253fb` | current_treasure_id | high | investigation | The treasure GETITEM gives: GETTREASURE sets it from the map object in play (GETTREASURE@0x00000) and GETITEM clears it (GETITEM@0x0018b). |
| `0x63da9c` | encounter_set_in_play | high | investigation | The encounter set in play, ENINIT's selector, from RMINIT's map_block_encounter_sets (FIELD@0x618) or a map script (MP01B0101@0x4e9); 0 means no encounters (FIELD@0x15c0). |
| `0x63dc91` | formation_in_play | high | investigation | The formation ESINIT loads, one of the drawn entry's two (SETENP@0x3f2). |
| `0x63dcda` | scripted_battle_case | high | investigation | The scripted party of the coming battle (1 to 25), 0 for the party in play; BTL runs CSINIT when it is set (BTL@0x000ab). Map events, scenes and ARENA set it. |
| `0x63df37` | current_item | high | investigation | The item ADDITEM, ITMES and DRAWTIP work on (ADDITEM@0x8; ITMES@0x0 compares it with each id). |
| `0x63df38` | item_count_change | high | investigation | How many of current_item ADDITEM adds (negative removes; ADDITEM@0x8). |
| `0x9f16fd` | condition_argument_id | high | investigation | The condition ADDILL and CALCILL apply; a map event sets it before calling ADDILL (MP06F0101@0x181). The cell before ILINIT. |
| `0x9f16fe` | condition_argument_levels | high | investigation | The levels ADDILL and CALCILL add to condition_argument_id, negative to take them away (MP06F0101@0x186: -5). |
| `0x9f31b7` | skill_message_skill | high | investigation | The skill whose description SSMES prints (SSMES@0x0); CHMODE2@0x3552 reads it. The cell before SSINIT. |
| `0x9f31b8` | skill_message_level | high | investigation | The level, 0 to 4, of skill_message_skill that SSMES prints (SSMES@0x0). |
| `0xa26f69` | routine_in_play | high | investigation | The routine RTN (an RTINIT row, BTL@0x31e9) or MVRTN (an SRINIT row, MVRTN@0x8e) follows. |
| `0xa26f6a` | routine_step_in_play | high | investigation | The routine step whose rule script runs (RTN@0x248, MVRTN@0x99). |
| `0xc25962` | battle_effect_in_play | high | investigation | The BTANINIT effect id SETEFFECT is building (SETEFFECT@0x73); BTANINIT branches on it. |
| `0xc25963` | battle_effect_slot_in_play | high | investigation | The effect slot (0 to 35) SETEFFECT is building; BTANINIT writes the battle_effect arrays at it (SETEFFECT@0x4b). |
| `0xc25964` | battle_animation_in_play | high | investigation | The BTANINIT2 animation ANIME plays, from ITINIT's G[0x9d65c9] for the action (BTL@0x1b89); 0 when the option G[0x3637] is set (ANIME@0x7). |
| `0xc48efe` | profile_shown | high | investigation | The CIINIT profile the 人物 book shows (INFO3@0x00fcd); CIMES@0x00000 writes its text. |
| `0xc49477` | glossary_selected_id | high | investigation | The glossary entry INFO2@0xfce shows; VIMES and the vimes chain branch on it to draw its text. |
| `0xc77b2d` | extra_game_id | high | investigation | The mini-game SYSTEM4 runs in mode 9: it calls extra_game_scripts[id] (SYSTEM4@0x00719). The fishing spots' map events set it to 1 (MPEV06F0401@0x001b9). |
| `0xc77b2e` | extra_game_scripts | high | investigation | Script ids of the mini-games by extra_game_id; INIT2 puts EXTRA1, the fishing game, in slot 1 (INIT2@0x00114). |
| `0xc7812e` | mounted_append_flags | high | investigation | Bit n is set by $n$AUTORUN: the appends mounted. ALLMAP@0x5c8, CGMODE@0x1e, HMODE@0x19 (six replay pages instead of five), MP07B0101@0x2eb, SAVE@0xfc4, SC0040 and SC4300 read it; GAMECLEAR@0x502 stores it. |

## story-flag

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x929` | story_flags | high | investigation | The game's flags, in a save. Each scene's own code sets [scene] to 1 at its start (SC0000@0x8f9 for scene 0, all 1334 SCINIT scenes); scenes set others (SC1170 sets [8114]), and each $n$AUTORUN sets [8114 + n]. SCJUMP's rules test them (most ask that their own scene's is not 1), as do objective_required_flags, glossary and quest flags (values there are a flag + 1). |
| `0x363a` | chapter | high | investigation | The chapter, 0 to 10: SC0000@0x902 sets 0 (序章), SC0020 2, SC0090 8 (第八章), SC0100 9 (終章), SC0140, SC0160 and SC0180 10; GAMESTART@0x1a5a sets 0. SCJUMP's rules test it. |
| `0x10f227` | item_obtained_flags | high | investigation | Saved: 1 once the item was obtained; ADDITEM@0x62..0x75 also stores it in the shared profile. |
| `0x1167be` | quest_states | high | investigation | Saved, by row (0 for quests 1 to 99, else current_inn_id) and slot (id % 100). Bit 0 taken (INN@0x728), bit 17 + party the party that took it or bit 20 where party_mode is set (INN@0x73f, 0x759), bit 1 cleared (QUESTCLEAR@0x1e6). |

## string-table

| address | name | conf | source | usage |
|---|---|---|---|---|
| `GS 0x1a8` | war_player_army_name | high | investigation | STINIT: the player's army's name (メンフィル軍 ...); a saved string. |
| `GS 0x1a9` | war_enemy_army_name | high | investigation | STINIT: the enemy's army's name; a saved string. |
| `GS 0x1b6` | speaker_names | high | investigation | CNINIT: the speaker's name on the name plate (SC0000@0x03c9a, HISTORY@0x0107e); the speaker id is the scene's G[0x3489]. |
| `GS 0x59e` | voice_setting_names | high | investigation | CVINIT: the setting's name, the descriptor shown until the unit has spoken, and the voice actress after CV: (CONFIG@0x023e7). Columns: 0=name, 1=descriptor, 2=voice_actress. |
| `GS 0x12a4` | unit_full_names | high | investigation | EBINIT: the unit's full name (セリカ・シルフィル); the table's leader. No script reads it. |
| `GS 0x168c` | unit_names | high | investigation | EBINIT: the name the game shows (DRAWCHP@0x00500, DRAWENP@0x0019e, INFO4@0x00acb, ADDTITLE@0x0004e and ten more). |
| `GS 0x1a74` | unit_description_lines | high | investigation | EBINIT: two lines on how the unit fights, drawn by the bestiary (INFO4@0x00f17) and the enemy panel (DRAWENP@0x00276). |
| `GS 0x2244` | area_names | high | investigation | AMINIT2: the area's name; FIELD@0x2b9 and TOWN@0x11 copy it into the saved place name GS[0x1a6]. |
| `GS 0x22a8` | town_site_names | high | investigation | SIINIT: the site's name (街を出る, ラギールの店 ...); no script indexes this base. |
| `GS 0x22c6` | arena_match_names | high | investigation | ARINIT: the match's name (ARENA@0x142e); an empty name keeps the id off the list (ARENA@0xa06). |
| `GS 0x232a` | arena_match_descriptions | high | investigation | ARINIT: five lines of description (ARENA@0x1441..0x149a). |
| `GS 0x251e` | quest_names | high | investigation | QUINIT's leader: the quest's name, 【組合】日向の薬草探し ... (INN@0xef2, 0x1155; QUESTCLEAR@0x2d). |
| `GS 0x25e6` | quest_texts | high | investigation | QUINIT: the description (0 to 2, the last the client, 【依頼主】) and the three hints 情報① to ③ (3 to 5), INN@0x1168 to 0x11e7. Columns: 0=description, 1=description_2, 2=client, 3=hint_1, 4=hint_2, 5=hint_3. |
| `GS 0x2a96` | map_floor_names | high | investigation | MPINIT: each floor's name, columns 1 to 20; FIELD@0x2cb copies it into the saved place name GS[0x1a6]. |
| `GS 0x2f78` | item_names | high | investigation | ITINIT's leader: the name of an item, spell, art or enemy action; every list and message prints it (RESULT@0xb0c). |
| `GS 0x5688` | item_notes | med | investigation | ITINIT: an author's note on 106 records ("MP016のミニゲームで獲得", "★セリカ"); no script reads it. |
| `GS 0x7d98` | condition_names | high | investigation | ILINIT: the name of each level (呪い1 to 呪い5), drawn by DRAWCHP@0x1719, DRAWENLIST@0x9e8, DRAWENP@0x618, DRAWCARD@0x36a. |
| `GS 0x811c` | skill_names | high | investigation | SSINIT: the name of each level (貫通Ⅰ to 貫通Ⅴ); a level with no name does not exist (CALCREVISE@0x2a5, SETCH@0xc2b). |
| `GS 0x8cd4` | skill_descriptions | high | investigation | SSINIT: the description of each level, 7 lines a level, cell 35 x skill + 7 x level + line. No script reads it: SSMES prints its own copy. |
| `GS 0xdedc` | title_names | high | investigation | TIINIT: the title's name; the table's leader. ADDTITLE@0x0005c announces it; CHECKTITLE@0x0007d skips a title without one. |
| `GS 0xe6ac` | title_description_lines | high | investigation | TIINIT: two lines on how the title fights, drawn by the enemy panel for a slot with a title (DRAWENP@0x00221). |
| `GS 0xf64c` | attack_element_names | high | investigation | AFINIT: the attack element names 1..17 (無属, 物理, 万能, 火炎 … 戦意, 抵抗1..7; DRAWTIP@0xb1). |
| `GS 0xf660` | defense_element_names | high | investigation | AFINIT: the defense element names 1..15 (… 神格, 魔神, 反万, 霊体, 不死, 創造; DRAWTIP@0xd3, INFO4@0x10ba). |
| `GS 0xf674` | class_names | high | investigation | AFINIT: the class names (重要アイテム, 小型武器, 魔術・治癒, 必殺・飛燕); CHMODE2@0x28b6, CHMODE3@0x1fe9. |
| `GS 0xf6b0` | class_short_names | high | investigation | AFINIT: the class short names (重要, 小型, 治癒, 飛燕); no script reads them. |
| `GS 0xf6ec` | class_equipment_notes | high | investigation | AFINIT: a class's equipment note (「盾」装備不可, 「剣」装備必須); no script reads them. |
| `GS 0xf728` | formation_names | high | investigation | AFINIT: the formation names (基本陣形 … 魔術陣形, 9..15 ？？陣形); CHMODE4@0x257e prints the party's. |
| `GS 0xf738` | formation_effect_lines | high | investigation | AFINIT: a formation's effect lines, two columns of seven (CHMODE4@0x2714). |
| `GS 0xf860` | objective_chapters | high | investigation | FAINIT's leader: the objective's chapter heading, 序章, 第一章 ... (INFO1@0xd0f). |
| `GS 0x10030` | objective_texts | high | investigation | FAINIT: the objective, drawn at INFO1@0xd26; an empty one is not listed (INFO1@0x849). |
| `GS 0x10800` | profile_names | high | investigation | CIINIT: the name in the 人物 book's list (INFO3@0x00948). |
| `GS 0x108c8` | glossary_titles | high | investigation | VIINIT's leader: the entry's title (【用語】 a term, 【神名】 a god); INFO2@0x99c lists only entries that have one. |
| `GS 0x10cb0` | terrain_names | high | investigation | SOINIT: the war game's terrain name, ids 0 to 9 (壁 平野 森 ...), SLG@0x302e. |
| `GS 0x10ce2` | troop_names | high | investigation | TPINIT: the troop's name (歩兵隊 ...), shown by SELACT and SLG. |
| `GS 0x10d00` | commander_names | high | investigation | CAINIT: the commander's name (リウイ ...). |
| `GS 0x10dc8` | tactic_names | high | investigation | RUINIT: the tactic's name (突撃 ...). |
| `GS 0x10e2c` | tactic_descriptions | high | investigation | RUINIT: the tactic's description (SELACT@0xb10). |
| `GS 0x10e90` | rank_names | high | investigation | GRINIT: the rank's name (勇士 豪将 英傑 覇王 戦神). |
| `GS 0x10e9a` | fishing_rod_texts | high | investigation | EX1INIT: rod k's name and up to three lines of its description (EXTRA1@0x07a58). Columns: 0=name, 1=line_1, 2=line_2, 3=line_3. |
| `GS 0x10eaa` | fishing_bait_texts | high | investigation | EX1INIT: bait k's name and up to three lines of its description (EXTRA1@0x079e9). Columns: 0=name, 1=line_1, 2=line_2, 3=line_3. |
| `GS 0x10eca` | fishing_rank_names | high | investigation | EX1INIT: the fishing ranks, 釣り初心者 to 釣りの神様 (EXTRA1@0x0758d). |
| `GS 0x10ed1` | fish_texts | high | investigation | EX1INIT: fish k's name and three lines of its description (EXTRA1@0x075ca, 0x02865). Columns: 0=name, 1=line_1, 2=line_2, 3=line_3. |
| `GS 0x1a4` | — | med | auto-shape | string-table (written by SC0100) |
| `GS 0x1a5` | — | med | auto-shape | string-table (written by SC0100) |
| `GS 0x1a6` | — | med | auto-shape | string-table (written by SC0100) |
| `GS 0x1b7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1b8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1b9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ba` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1bb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1bc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1bd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1c8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ca` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ce` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1cf` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1d9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1da` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1db` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1dc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1dd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1df` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1e9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ea` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1eb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ec` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ed` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ee` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ef` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1f9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1fa` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1fb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1fd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1fe` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x1ff` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x200` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x201` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x202` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x203` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x204` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x205` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x206` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x207` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x208` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x209` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x20f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x210` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x211` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x214` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x215` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x216` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x219` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x21a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x21b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x21d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x21f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x220` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x221` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x222` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x223` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x224` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x225` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x226` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x227` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x228` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x229` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x22f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x230` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x231` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x232` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x233` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x234` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x237` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x238` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x239` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x23f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x240` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x241` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x242` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x243` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x244` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x245` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x246` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x247` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x248` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x249` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x24f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x252` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x253` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x254` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x255` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x256` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x257` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x258` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x259` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x25b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x25c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x25d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x25e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x261` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x262` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x263` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x265` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x267` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x268` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x269` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x26a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x26b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x26c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x26d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x26e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x26f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x270` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x271` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x272` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x273` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x274` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x275` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x276` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x277` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x278` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x279` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x27f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x280` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x281` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x282` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x283` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x284` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x285` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x286` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x287` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x288` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x289` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x28a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x28b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x28c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x28d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x28e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x28f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x290` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x291` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x292` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x293` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x294` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x295` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x296` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x297` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x298` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x299` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x29f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2a9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2aa` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ab` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ad` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2b6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ba` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2bb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2bc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2bd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2be` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2c7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2cd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ce` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2cf` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2d9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2da` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2e9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ea` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2eb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ed` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ee` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ef` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2f9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2fa` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2fb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2fc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2fd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2fe` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x2ff` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x300` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x301` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x302` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x303` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x304` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x305` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x307` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x30a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x30b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x30c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x30d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x30e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x348` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x349` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x34a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x34b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x351` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x352` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x353` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x354` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x356` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x357` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x358` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x359` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x35a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x35b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x35c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x35d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x35e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x35f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x360` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x361` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x362` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x363` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x364` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x365` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x366` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x367` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x368` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x369` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x36d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x371` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ab` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ac` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ad` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ae` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3af` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3b7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3dd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3de` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3df` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3e9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ea` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3eb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ec` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ed` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ee` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3ef` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f0` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f1` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f2` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f3` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f4` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f5` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f6` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f7` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f8` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3f9` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3fa` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3fb` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3fc` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3fd` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x3fe` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x40f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x410` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x411` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x412` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x413` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x414` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x427` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x428` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x429` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x42a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x42b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x42e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x42f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x431` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x432` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x433` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x434` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x435` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x436` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x437` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x438` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x439` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x43b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x43d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x441` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x442` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x443` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x444` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x445` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x446` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x447` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x448` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x449` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x44a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x44b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x44c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x44f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x454` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x455` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x459` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x45a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x45b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x45d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x45e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x45f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x463` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x464` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x465` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x466` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x468` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x469` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x46b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x46c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x46d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x53a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x53b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x53c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x53d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x53e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x53f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x540` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x541` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x542` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x543` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x544` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x545` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x546` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x547` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x548` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x549` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x54a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x54b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x54c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x54d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x54e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x54f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x550` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x551` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x552` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x553` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x554` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x555` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x556` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x557` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x558` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x559` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x55a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x55b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x55c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x55d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x55e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x55f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x560` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x561` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x562` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x563` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x564` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x565` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x566` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x567` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x568` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x569` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x56a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x56b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x56c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x56d` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x56e` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x56f` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x570` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x571` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x572` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x573` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x574` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x575` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x577` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x578` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x579` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x57a` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x57b` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x57c` | — | med | auto-shape | string-table (written by CNINIT) |
| `GS 0x5a1` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a2` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a3` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a4` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a5` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a6` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a7` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a8` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5a9` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5aa` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ab` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ac` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ad` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ae` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5af` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b0` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b1` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b2` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b3` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b4` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b5` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b6` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b7` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b8` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5b9` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ba` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5bb` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5bc` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5bd` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5be` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5bf` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c0` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c1` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c2` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c3` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c4` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c5` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c6` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c7` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c8` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5c9` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ca` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5cb` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5cc` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5cd` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ce` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5cf` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d0` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d1` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d2` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d3` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d4` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d5` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d6` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d7` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d8` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5d9` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5da` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5db` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5dc` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5dd` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5de` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5df` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e0` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e1` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e2` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e3` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e4` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e5` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e6` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e7` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e8` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5e9` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ea` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5eb` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ec` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ed` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ee` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ef` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f0` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f1` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f2` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f3` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f4` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f5` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f6` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f7` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f8` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5f9` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5fa` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5fb` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5fc` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5fd` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5fe` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x5ff` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x600` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x601` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x602` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x603` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x604` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x605` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x606` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x607` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x608` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x609` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x60a` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x60b` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x60c` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x60d` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x60e` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x60f` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x610` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x611` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x612` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x613` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x614` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x615` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x616` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x617` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x618` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x619` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x61a` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x61b` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x61c` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x61d` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x61e` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x61f` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x620` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x621` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x622` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x623` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x624` | — | med | auto-shape | string-table (written by CVINIT) |
| `GS 0x1233` | — | med | auto-shape | string-table (written by BUNKI) |
| `GS 0x12a2` | — | med | auto-shape | string-table (written by SAVE) |
| `GS 0x12a3` | — | med | auto-shape | string-table (written by SC5500) |
| `GS 0x12a5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12a7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12b9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12bb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12be` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12bf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12c9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12de` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12df` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x12ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12ed` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12ee` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12ef` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f7` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12f9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12fd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x12ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1302` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1303` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1304` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1307` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1308` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1309` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x130f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1310` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1311` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1312` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1313` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1314` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1315` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1316` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1317` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1318` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1319` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x131f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1320` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1321` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1322` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1325` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1326` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1327` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1328` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1329` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x132a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x132b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x132d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x132e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x132f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1330` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1331` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1332` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1333` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1334` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1335` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1336` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1337` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1338` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1339` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x133a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x133b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x133c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x133d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1340` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1341` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1342` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1343` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1344` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1345` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1346` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1347` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1349` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x134a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x134b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x134c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x134f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1350` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1351` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1353` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1355` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1356` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1357` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1358` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1359` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x135a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x135b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x135c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x135d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x135e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x135f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1360` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1361` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1362` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1363` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1364` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1365` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1366` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1367` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1368` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1369` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x136a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x136b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x136c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x136d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x136e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x136f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1370` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1371` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1372` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1373` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1374` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1375` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1376` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1377` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1378` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1379` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x137a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x137b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x137c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x137d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x137e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x137f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1380` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1381` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1382` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1383` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1384` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1385` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1386` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1387` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1388` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1389` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x138a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x138c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x138d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x138e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1390` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1391` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1392` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1393` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1394` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1395` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1396` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1397` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1398` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1399` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x139b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x139e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x139f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13b4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13b5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13bb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13be` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13bf` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x13c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13de` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13df` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13ef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13f9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x13fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1436` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1437` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1438` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1439` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x143f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1440` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1441` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1442` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1444` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1445` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1446` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1447` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1448` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1449` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x144a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x144b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x144c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x144d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x144e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x144f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1450` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1451` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1452` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1453` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1454` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1455` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1456` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1457` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1458` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1459` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x145a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x145b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x145f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1499` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x149a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x149b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x149c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x149d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x149e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14a0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14a1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14a2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14a3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14a4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14a5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14da` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14db` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14de` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14df` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14e0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e7` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14fd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x14fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x14ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1500` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1501` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1502` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1515` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1516` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1517` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1518` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1519` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x151c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x151d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x151f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1520` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1521` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1522` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1523` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1524` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1525` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1526` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1527` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1529` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x152b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x152f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1530` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1531` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1532` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1533` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1534` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1535` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1536` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1537` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1538` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1539` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x153a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x153d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1542` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1543` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1547` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1548` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1549` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x154b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x154c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x154d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1551` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1552` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1553` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1554` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1556` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1557` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1559` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x155a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x155b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1628` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1629` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x162a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x162b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x162c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x162d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x162e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x162f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1630` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1631` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1632` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1633` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1634` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1635` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1636` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1637` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1638` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1639` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x163a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x163b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x163c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x163d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x163e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x163f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1640` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1641` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1642` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1643` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1644` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1645` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1646` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1647` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1648` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1649` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x164a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x164b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x164c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x164d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x164e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x164f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1650` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1651` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1652` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1653` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1654` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1655` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1656` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1657` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1658` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1659` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x165a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x165b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x165c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x165d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x165e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x165f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1660` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1661` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1662` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1663` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1664` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1665` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1666` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1667` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1668` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1669` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x166a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x168b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x168d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x168e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x168f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1690` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1691` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1692` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1693` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1696` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1697` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1698` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1699` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x169a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x169b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x169e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16b9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16bb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16be` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16bf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16c9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16d5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16d6` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16d7` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16d8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16d9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16da` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16db` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16dc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x16de` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16df` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16e7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16eb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16f9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16fa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16fb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16fc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16fd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x16ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1700` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1701` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1702` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1703` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1704` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1705` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1706` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1707` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1708` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1709` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x170a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x170d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x170e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x170f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1710` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1711` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1712` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1713` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1715` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1716` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1717` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1718` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1719` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x171f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1720` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1721` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1722` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1723` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1724` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1725` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1728` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1729` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x172a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x172b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x172c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x172d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x172e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x172f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1731` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1732` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1733` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1734` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1737` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1738` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1739` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x173b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x173d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x173e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x173f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1740` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1741` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1742` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1743` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1744` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1745` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1746` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1747` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1748` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1749` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x174f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1750` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1751` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1752` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1753` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1754` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1755` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1756` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1757` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1758` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1759` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x175a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x175b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x175c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x175d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x175e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x175f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1760` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1761` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1762` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1763` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1764` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1765` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1766` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1767` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1768` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1769` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x176a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x176b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x176c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x176d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x176e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x176f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1770` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1771` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1772` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1774` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1775` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1776` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1778` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1779` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x177a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x177b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x177c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x177d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x177e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x177f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1780` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1781` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1783` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1786` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1787` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1788` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1789` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x178a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x178b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x178c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1790` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1791` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1792` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1793` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1794` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1796` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1797` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1798` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1799` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x179a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x179b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x179c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x179d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x17a3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x17a4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x17a5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x17a6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17a7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17a8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17a9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17aa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17af` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17b0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17b9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17bb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17be` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17bf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17c9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17d1` | — | med | auto-shape | string-table (written by EBINIT) |
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
| `GS 0x17dd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x17e4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x181e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x181f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1820` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1821` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1827` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1828` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1829` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x182f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1830` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1831` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1832` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1833` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1834` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1835` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1836` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1837` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1838` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1839` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x183a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x183b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x183c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x183d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x183e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x183f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1840` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1841` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1842` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1843` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1847` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1881` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1882` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1883` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1884` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1885` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1886` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1888` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1889` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x188a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x188b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x188c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x188d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18b9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18ba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18bb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18bc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18bd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18be` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18bf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18c8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18c9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18ca` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18cb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18cc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18cd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18ce` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18cf` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18d0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18d1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18d2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18d3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x18d4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18e9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18ea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18fd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18fe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x18ff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1900` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1901` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1904` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1905` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1907` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1908` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1909` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x190a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x190b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x190c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x190d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x190e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x190f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1911` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1913` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1917` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1918` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1919` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x191a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x191b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x191c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x191d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x191e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x191f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1920` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1921` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1922` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1925` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x192a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x192b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x192f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1930` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1931` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1933` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1934` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1935` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1939` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x193a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x193b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x193c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x193e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x193f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1941` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1942` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1943` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a10` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a11` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a12` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a13` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a14` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a15` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a16` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a17` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a18` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a19` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a1a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a1b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a1c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a1d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a1e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a1f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a20` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a21` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a22` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a23` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a24` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a25` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a26` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a27` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a28` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a29` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a2a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a2b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a2d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a2e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a2f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a30` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a31` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a32` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a33` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a34` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a35` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a36` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a37` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a38` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a39` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a3a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a3b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a3c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a3d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a3e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a3f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a40` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a41` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a42` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a43` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a44` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a45` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a46` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a47` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a48` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a49` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a4a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a4b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a4d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a4e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a4f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a50` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a51` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a52` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a76` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a77` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a7a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a7b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a7c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a7d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a88` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a89` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a91` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a9d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1a9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aa4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aa5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aa8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aa9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aaa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aaf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ab2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ab3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ab6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ab7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1abc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1ac2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1ac6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1acb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ad0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ad1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ad6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ad7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ad8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ad9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ae4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ae5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1af9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1afa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1afb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1afe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1aff` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b24` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b25` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b3a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b3b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b3e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b3f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b42` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b43` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b46` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b47` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b4a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b4b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b4c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b4d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b4e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b4f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b50` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b51` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b52` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b53` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b54` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b55` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b56` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b57` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b58` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b59` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b5a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b5b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b5c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b5d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b5e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b5f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b60` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b62` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b63` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b64` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b65` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b66` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b67` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b68` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b69` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b6a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b6b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b6c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b6d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b70` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b71` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b76` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b77` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b78` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b79` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b7a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b7b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b7c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b7e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b7f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b80` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b81` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b82` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b83` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b86` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b87` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b88` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b89` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b8a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b8b` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b8c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b8d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1b8e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b8f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b91` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b92` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b93` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b94` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b95` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b96` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b9a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b9b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b9d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1b9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ba7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1baf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bb0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bb2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bb4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bb6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bb8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bbb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bbe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bbf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bc0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bc1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bc2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bc3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bc4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bc5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bcc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bcd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bd2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bd3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bd6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bd7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bd8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bd9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bda` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bdb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bdc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bdd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bde` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1be8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1be9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bea` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1beb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bec` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bed` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bee` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bef` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bf0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bf1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1bf2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bf9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bfa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bfb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bfc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bfd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bfe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1bff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c01` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c04` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c05` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c06` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c07` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c08` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c09` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c0a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c0b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c0c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c0d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c0e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c0f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c10` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c11` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c12` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c13` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c14` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c15` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c16` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c17` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c18` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c19` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c1a` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c1c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c1d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c1e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c1f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c20` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c21` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c22` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c23` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c24` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c25` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c26` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c27` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c28` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c29` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c2a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c2b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c2e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c30` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c31` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c32` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c33` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c34` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c35` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c36` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c37` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c38` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c39` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c3a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c3b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c3c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c3d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c3e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c3f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c40` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c41` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c44` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c45` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c46` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c47` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c48` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c49` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c4c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c4d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c4e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c4f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c50` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c51` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c52` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c53` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c54` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c55` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1c56` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c57` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c58` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c59` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c5a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c5b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c5c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c5d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c5e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c5f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c62` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c63` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c68` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c69` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c6a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c6b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c6c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c6d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c6e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c6f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c70` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c71` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c72` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c73` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c74` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c75` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c7c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c7d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c7e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c7f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c80` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c81` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c82` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c83` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c84` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c85` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c88` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c89` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c8a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c8b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c8c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c8d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c8e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c8f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c91` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c92` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c94` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c96` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1c97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ca2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ca4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ca5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ca6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ca7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ccf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cd9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cda` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cdb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cdc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cdd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cde` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cdf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ce2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ce3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ce4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ce6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ce7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ce8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cf9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cfa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cfb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cfc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cfd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cfe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1cff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d01` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d04` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d05` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d06` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d07` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d08` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d0a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d0c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d0d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d0e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d0f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d10` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1d16` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1d17` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1d1c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1d1d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1d1e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d1f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d20` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d21` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d22` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d23` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d24` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d25` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d9a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d9b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d9d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1d9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1daa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dab` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dac` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dad` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dae` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1daf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1db9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dbb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dbc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dbd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dbe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dbf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dc9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dcb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dcc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dcd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dcf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dd9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dda` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ddb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ddc` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1ddd` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1dde` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1ddf` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1de0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1de1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1de2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1de3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1dea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1deb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ec9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eca` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ecb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ecc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ecd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ece` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ecf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ed9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eda` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1edb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1edc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1edd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ede` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1edf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ee9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eea` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eeb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eec` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eed` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eee` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eef` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef8` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1ef9` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1efa` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1efb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1efc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1efd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1efe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1eff` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f00` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f01` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f02` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f03` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f04` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f05` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f26` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f28` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f29` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f2a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f2b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f2c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f2d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f2e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f2f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f30` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f31` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f56` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f57` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f58` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f59` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f5a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f5b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f5c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f5d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f5e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f5f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f64` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f65` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f66` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f6a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f6b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f6c` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f6d` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f6e` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f6f` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f70` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f71` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f72` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f73` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f74` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f75` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1f76` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f77` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f78` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f79` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f7a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f7b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f7e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f7f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f82` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f83` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f8a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f8b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f8c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f8d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f8e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f8f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f90` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f91` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f92` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f93` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f94` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f95` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f96` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f97` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f98` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f99` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f9a` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f9b` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f9c` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f9d` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f9e` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1f9f` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fa6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fa7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fb0` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fb1` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fb2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fb3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fba` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fbb` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fbc` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fbd` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fbe` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fbf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fc2` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fc3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fc4` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fc5` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fc6` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fc7` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fce` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fcf` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x1fd0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd3` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd4` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd5` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd8` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fd9` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fda` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fdb` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fde` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fdf` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fe0` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fe1` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fe2` | — | med | auto-shape | string-table (written by EBINIT) |
| `0x1fe3` | — | med | auto-shape | string-table (written by EBINIT) |
| `GS 0x2245` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2246` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2247` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2248` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2249` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x224a` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x224b` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x224c` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x224d` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x224e` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x224f` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2250` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2251` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2252` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2253` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2254` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2255` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2256` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2257` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2258` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2259` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x225a` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x225b` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x225c` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x225d` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x225f` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2260` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2261` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2262` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2263` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2264` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2265` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2266` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2267` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2268` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2269` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x226b` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x226d` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x226f` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2276` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2277` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2278` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2279` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x227a` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x227b` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x227c` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x227d` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x227e` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x227f` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2280` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2281` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2282` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2283` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2284` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2285` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2286` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2287` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2288` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x2289` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x228c` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x228d` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x228e` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x228f` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2290` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2291` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2292` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2293` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2294` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2295` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2296` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2297` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2298` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x2299` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x229a` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x229b` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x229c` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x229d` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x229e` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x229f` | — | med | auto-shape | string-table (written by AMINIT2) |
| `0x22a0` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a1` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a2` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a3` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a4` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a5` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a6` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a7` | — | med | auto-shape | string-table (written by AMINIT2) |
| `GS 0x22a9` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22aa` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22ab` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22ac` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22ad` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22ae` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22af` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b0` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b1` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b2` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b3` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b4` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b5` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b6` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22b7` | — | med | auto-shape | string-table (written by SIINIT) |
| `GS 0x22c7` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22c8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22c9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22ca` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22cb` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22cc` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22cd` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22ce` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22cf` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d4` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d6` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d7` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22d9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22da` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22db` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22dc` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22dd` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e3` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e4` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e6` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e7` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22e9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22ea` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x22f0` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f1` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f2` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f3` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f4` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f5` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f6` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x22f7` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2315` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2316` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2317` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2318` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2319` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x231a` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x231b` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x231c` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x231d` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x232f` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2330` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2332` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2334` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x2335` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2337` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2339` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x233a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x233c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x233e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x233f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2341` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2343` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2344` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2346` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2348` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2349` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x234b` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x234d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x234e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2350` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2352` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2353` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2355` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2357` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2358` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x235a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x235c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x235d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x235f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2370` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2371` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2373` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2375` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2376` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2378` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x237a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x237b` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x237d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x237f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2380` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2382` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2384` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2385` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2387` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2389` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x238a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x238c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x238e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x238f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2391` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2393` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2394` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2396` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2398` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2399` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x239b` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x239d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x239e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23a0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23bb` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23bc` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23be` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23c0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23c1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23c3` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23c5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23c6` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23c8` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23ca` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23cb` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23cd` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23cf` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23d0` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23d2` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23d4` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23d5` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23d7` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23d9` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23da` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23dc` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23de` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23df` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23e1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23fc` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23fd` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x23ff` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2401` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2402` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2404` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2406` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2407` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2409` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x240b` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x240c` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x240e` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2410` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2411` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2413` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2415` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2416` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2418` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x241a` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x241b` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x241d` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x241f` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2420` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x2422` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24b5` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24b6` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24b8` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24b9` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24ba` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24bb` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24bd` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24be` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24bf` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c0` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c2` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c3` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c4` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c5` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c6` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c7` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c8` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24c9` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24ca` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24cb` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24cc` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24cd` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24ce` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24cf` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d0` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d1` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d2` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d3` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d4` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d6` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d7` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d8` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24d9` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24db` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24dc` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24dd` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24de` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24e0` | — | med | auto-shape | string-table (written by ARINIT) |
| `0x24e1` | — | med | auto-shape | string-table (written by ARINIT) |
| `GS 0x251f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2520` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2522` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2523` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2525` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2526` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2529` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2533` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2534` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2536` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2537` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2539` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x253a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x253c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x253d` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x253f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2541` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2542` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2560` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2566` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2583` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2584` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2585` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2586` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2587` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2588` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2589` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x258a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x258b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x258c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x258d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x258e` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x258f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2590` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2591` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2598` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2599` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x259b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x259c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25ec` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25ed` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25ee` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25ef` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25f0` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25f2` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25f3` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25f4` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25f5` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25f6` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25fe` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x25ff` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2600` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2601` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2602` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2604` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2605` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2606` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2607` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2608` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2610` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2611` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2612` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2613` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2614` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2616` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2617` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2618` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2619` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x261a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x261b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2628` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2629` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x262a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x262b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x262c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x262d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2664` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2665` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2666` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2667` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2668` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x266a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x266b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x266c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x266d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x266e` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x266f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2676` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2677` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2678` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2679` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x267a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x267c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x267d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x267e` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x267f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2680` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2688` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2689` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x268a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x268b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x268c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x268e` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x268f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2690` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2691` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2692` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x269a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x269b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x269c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x269d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x269e` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26a0` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26a1` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26a2` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26a3` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26a4` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26ac` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26ad` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26ae` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26af` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26b0` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26b8` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26b9` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26ba` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26bb` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26bc` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26be` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26bf` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26c0` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26c1` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x26c2` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2772` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2773` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2774` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2775` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2776` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2796` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2797` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2798` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2799` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x279a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2844` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2845` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2846` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2847` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2848` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2849` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x284a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x284b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x284c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x284d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x284e` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x284f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2850` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2851` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2853` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2854` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2855` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2856` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2857` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2858` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2859` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x285a` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x285b` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x285c` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x285d` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x285f` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2860` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2861` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2862` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2863` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2864` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2865` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2866` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2867` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2868` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2869` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x286a` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x286b` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x286c` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x286d` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x286e` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x286f` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2870` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2871` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2872` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2873` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2874` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2875` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2876` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2877` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2878` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2879` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x287a` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x287b` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x287c` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x287d` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x287e` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2880` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2881` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2882` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2883` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2884` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2885` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2886` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2887` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2888` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2889` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x288a` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x288c` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x288d` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x288e` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x288f` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2890` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2892` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2893` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2894` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2895` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2896` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x2898` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2899` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x289a` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x289b` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x289c` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c2` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c3` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c4` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c5` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c6` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c7` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c8` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28c9` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28ca` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28cb` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28cc` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28d4` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28d5` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x28d7` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x28da` | — | med | auto-shape | string-table (written by QUINIT) |
| `0x28db` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x28dd` | — | med | auto-shape | string-table (written by QUINIT) |
| `GS 0x2ab0` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ab1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ac9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2aca` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ae9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2aea` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2afb` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2afc` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b14` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b15` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b16` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b17` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b2d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b2e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b2f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b30` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b46` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b47` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b48` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b49` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b4a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b4b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b4c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b4d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b4e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b4f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b50` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b51` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b52` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b53` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b54` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b55` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b56` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b57` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b58` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b59` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b5f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b60` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b61` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b62` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b63` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b78` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b79` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b7a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b7b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b7c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b7d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b7e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b7f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b80` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b81` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b82` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b91` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b92` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b93` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b94` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b95` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b96` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2b97` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2baa` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bab` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bac` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bad` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bae` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2baf` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bc3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bc4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bc5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bc6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bc7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bdc` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bdd` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bde` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bdf` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2be0` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2be1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bf5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bf6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bf7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bf8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2bf9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c0e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c27` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c28` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c40` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c41` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c42` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c59` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c5a` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2c5b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c72` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c73` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c74` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c75` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c8b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c8c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2c8d` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2ca4` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2ca5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ca6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ca7` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2ca8` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2ca9` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2caa` | — | med | auto-shape | string-table (written by MPINIT) |
| `0x2cab` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cac` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cbd` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cd6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cd7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cd8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cef` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf0` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cf9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cfa` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cfb` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2cfc` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d08` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d3a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d53` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d6c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d6d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d6e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d85` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d86` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d87` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d9e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2d9f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2da0` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2da1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2db7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2db8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2db9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dba` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dd0` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dd1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dd2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dd3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dd4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dd5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2de9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dea` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2deb` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2dec` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e02` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e03` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e04` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e05` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e06` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e1b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e1c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e1d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e1e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e1f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e20` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e34` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e35` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e36` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e66` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e67` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e68` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e69` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e6a` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e6b` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e6c` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e6d` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e6e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e6f` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2e98` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2eca` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ecb` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ecc` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ecd` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ece` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ecf` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed0` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed1` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed2` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed3` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed4` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed5` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed6` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed7` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed8` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2ed9` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2eda` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2edb` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2edc` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2edd` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2f2e` | — | med | auto-shape | string-table (written by MPINIT) |
| `GS 0x2f79` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f7a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f7b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f7c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f7d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f7e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f7f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f81` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f82` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f83` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f84` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f85` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f86` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f87` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f88` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f89` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f8a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f8b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f8c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f8d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f8e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f8f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f90` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f91` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f92` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f93` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f94` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f95` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f96` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f97` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f98` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f99` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2f9f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fa9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fae` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2faf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fb5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fb6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fbf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fc9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fca` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fcb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fcc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fcd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fce` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fcf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fd9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fdd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fde` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fdf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fe9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2feb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ff9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ffa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ffb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ffc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ffd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2ffe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x2fff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x300f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3010` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3011` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3012` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3013` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3014` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3015` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3016` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3017` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3018` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3019` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x301a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x301b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x301c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x301d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x301e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x301f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3041` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3042` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3043` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3046` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3047` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3048` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x304b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x304c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x304d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3050` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3051` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3052` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3055` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3056` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3057` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x305a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x305b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x305c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x305f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3060` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3064` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3065` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3069` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x306a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3073` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3074` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3075` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3076` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3077` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3078` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3079` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x307a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x307d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3087` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3088` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3089` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x308a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x308b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x308c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x308d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x308e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x308f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3090` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3091` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x30a5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x30a6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x30a7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3109` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x310a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x310b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x310c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x310d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x310e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x310f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3110` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3111` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3112` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3113` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3114` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3115` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3116` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3117` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3118` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3119` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x311a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x311b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x311c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x311d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x311e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x311f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3120` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3121` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3122` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3123` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3124` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3125` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3126` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3128` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3129` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x312a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x312b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x312c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x312d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x312e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x312f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3130` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3131` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3132` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3133` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3134` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3135` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3136` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3137` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3138` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3139` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x313a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x313b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x313c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x313d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x313e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x313f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3140` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3141` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3142` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3143` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x314f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3150` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3151` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3152` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3153` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3154` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3155` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3156` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3157` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3159` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x315b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x315c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x315d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x315e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x315f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3160` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3161` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3163` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x316d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x316e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x316f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3170` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3171` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3172` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3174` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31d8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31db` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31dc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31dd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31de` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31df` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x31e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3361` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3362` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3363` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3364` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3365` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3366` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3367` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3368` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3369` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x336a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x336b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x336c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x336d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x336e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3392` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3393` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3394` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3395` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3396` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3397` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3398` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3399` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33c5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33c7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33c8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33c9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ca` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33cb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33cc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33cd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33ce` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33cf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33d0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x33d1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3429` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x342a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x342b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x342c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x342d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x342e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3430` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3431` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3432` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3433` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3434` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3435` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x345b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x345c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x345d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x345e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x345f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3460` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3461` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3462` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3463` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3464` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3465` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3466` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3467` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3468` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3469` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x346a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x346b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x346c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x346d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x346e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x346f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3470` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3471` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3472` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3473` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3474` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x348c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x348d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x348e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3490` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3491` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3492` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3493` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3494` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3495` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3496` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3497` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3498` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x34bf` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x34c0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34f1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34f2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34f3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34f4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34f5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x34f6` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x34f7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x350a` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x350b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x350c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x350d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x350e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x350f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3510` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3523` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3526` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3527` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3528` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3529` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x352a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x352b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x352c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x352d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x352e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3555` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3556` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3557` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3558` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3559` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x355a` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x355b` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x355c` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x355d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x355f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3560` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35b9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35ba` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35bb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35bc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35bd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35be` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35bf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35c0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35c1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35c2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35c3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35c4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35ea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35eb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35ee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x35ef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x361d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x361e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x361f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3620` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3621` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3622` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3623` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3624` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3625` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3628` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3629` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x364f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3653` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3749` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x374f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3753` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3754` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3755` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3756` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3757` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3758` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3759` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x375a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x375b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x375c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x375d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3767` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3768` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3769` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3771` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3772` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3773` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3774` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x377b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x377c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x377d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x377e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3785` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3786` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3787` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3788` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x378f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3790` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3791` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3792` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3793` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3794` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3799` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x379a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x379b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x379c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37a3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37a4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37a5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37ac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37ad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37ae` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37af` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37b7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37b8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37b9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37c1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37c2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x37c3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3811` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3812` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3813` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3814` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3815` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3816` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3817` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x381b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x381c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x381d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x381e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x381f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3820` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3821` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3825` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3826` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3827` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3828` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x382f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3830` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3831` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3832` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x383a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x383b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x383c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x383d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3843` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3844` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3845` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3846` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x384d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x384e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x384f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3850` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3857` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3858` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3859` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x385a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3861` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3862` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3863` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3864` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x386b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x386c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x386d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38d9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38da` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38db` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38dc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38dd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38de` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38df` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38e0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38e1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38e2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x38e6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x393d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3942` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3943` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a69` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a6f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a70` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a73` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a74` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3a76` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3acd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ace` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3acf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ad9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ada` | — | med | auto-shape | string-table (written by ITINIT) |
| `0x3adb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3adc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3add` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ade` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3adf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ae9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aeb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3af9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3afc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3afd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3afe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3aff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b00` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b01` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b03` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b04` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b05` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b06` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b07` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b08` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b09` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b0a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b0b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b0c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b0d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b0e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b0f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b10` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b11` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b12` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b13` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b14` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b15` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b16` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b17` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b18` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b31` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b32` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b33` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b34` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b35` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b36` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b37` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b38` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b39` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b3a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b45` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b46` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b47` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b48` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b49` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b4a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b4b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b4c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b4d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b4e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b4f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b50` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b59` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b5a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b5b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b5c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b5d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b5e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b5f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b60` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b61` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b62` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b63` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b6d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b6e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b6f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b70` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b73` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b74` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b75` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b81` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b82` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b83` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b84` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b85` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b86` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b87` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b88` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b89` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b8a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b8b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b8c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b8d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b95` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b96` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b97` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b98` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b99` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b9a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b9b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b9c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b9d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b9e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3b9f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ba0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3ba9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3baa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bae` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3baf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bb0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bb1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bb2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bb3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bb4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bbd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bbe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bbf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bc0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bc1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bc2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bc3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bc4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bd9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bda` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bdb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3be5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3be6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3be7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3be8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3be9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3beb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bf0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bf1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bf9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bfa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bfb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bfc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bfd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bfe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3bff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c00` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c01` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c0d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c0e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c0f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c10` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c11` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c12` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c13` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c14` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c15` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c16` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c17` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c18` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c21` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c22` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c23` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c24` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c25` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c26` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c27` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c28` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c29` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c2a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c2b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c2c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c35` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c36` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c37` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c38` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c39` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c3a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c3b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c3c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c3d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c3e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c3f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c40` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3c41` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f19` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f1a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f1b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f1c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f1d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f1e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f1f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f20` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f21` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f22` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f2d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f2e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f2f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f30` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f31` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f32` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f33` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f34` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f35` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f36` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f37` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f41` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f42` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f43` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f44` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f45` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f46` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f47` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f48` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f49` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f4a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f4b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f55` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f56` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f57` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f58` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f59` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f5a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f5b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f5c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f5d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f5e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f5f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f60` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f61` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f69` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f6a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f6c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f6d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f6e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f6f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f70` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f7d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f7e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f7f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f80` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f81` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f82` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f83` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f84` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f85` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f86` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f91` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f92` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f93` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f94` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f95` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f96` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f97` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f98` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f99` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f9a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f9b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f9c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f9d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3f9e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fa5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fa6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fa7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fa8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fa9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3faa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fb9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fba` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fbb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fbc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fbd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fbe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fbf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fc0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fc1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x3fc2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4307` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4308` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4309` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x430a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4313` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4314` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4319` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x431a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x431b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x431c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x431d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x431e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x431f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4320` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4321` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4322` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4323` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4324` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4325` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4326` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4327` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4328` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x432b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x432c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x432d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x432e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4337` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4338` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4339` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x433a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x433b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4365` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4366` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4367` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4368` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4369` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x436a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x436b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x436c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x436f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4370` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4371` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4372` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4373` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4374` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4375` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4376` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4379` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x437a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x437b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x437c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x437d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x437e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x437f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4380` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4397` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4398` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4399` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x439a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x439b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x439c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x439d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x439e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43a8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ae` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43af` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43b9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ba` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43bb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43bc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43c9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ca` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43cb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43cc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43cd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ce` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43cf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43d9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43da` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43dd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43de` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43df` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43eb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43f1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43f2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43f3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43f4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43f6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43f7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43fb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43fc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43fd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43fe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x43ff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4400` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4401` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4402` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4403` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4405` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4406` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4407` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4408` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4409` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x440a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x440b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x440c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x440f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4410` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4411` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4412` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4413` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4414` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4415` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4416` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x44fb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x44fc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x44fd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x44fe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x44ff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4500` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4513` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4514` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4551` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4552` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4553` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4554` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4555` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4556` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4557` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4687` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4688` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4689` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x468d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x468e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x468f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4690` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4691` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4692` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4693` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4694` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4695` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4696` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4697` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4698` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4699` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x469a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x469c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x469d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x469f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46a9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46aa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46ab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46ac` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46ea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46eb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46ec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46f8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46f9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46fa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46fd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46fe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x46ff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4700` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4701` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x474d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4750` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4751` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4752` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4757` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4758` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4759` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x475c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x475d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x475e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4761` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4762` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4763` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x476b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x476c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47b9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47ba` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47bb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47bc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47bd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47be` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47bf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47c3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47c4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47c5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47c6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47c9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47cb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47cc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47cd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47ce` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47cf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47d9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47da` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47db` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47dc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47de` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47df` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47e9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47ea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47eb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47ec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47f7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47f8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x47f9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4801` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4802` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4805` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4815` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4816` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4817` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4818` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x481a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x481b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x481c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x481d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x481f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4820` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4821` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4822` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4824` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4825` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4826` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4827` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4829` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x482a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x482b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x482e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x482f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4830` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4833` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4834` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4835` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4836` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4838` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4839` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x483a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x483b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x483c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4847` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4848` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4849` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x484a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x484c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x484d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x484e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x484f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4851` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4852` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4853` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4854` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4856` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4857` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4858` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4859` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x485b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x485c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x485d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x485e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4860` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4861` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4862` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4863` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4865` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4866` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4867` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4868` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x486a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x486b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x486c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x486d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ad2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ad3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4adb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4adc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4add` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ade` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4adf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ae0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b35` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b36` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b37` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b38` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b3c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b67` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b68` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b69` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b99` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4b9c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ba1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ba2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ba6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ba8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4ba9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4baa` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bab` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bad` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bae` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4baf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bb8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bbc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bfd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bfe` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4bff` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c00` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c01` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c02` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c03` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c04` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c05` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c07` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c08` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c09` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c0a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c0b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c11` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c12` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c13` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c14` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c16` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c1b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c20` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c61` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c62` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c63` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c64` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c65` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c6c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c6d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c75` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c76` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c77` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c78` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c7a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c7b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c7c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c7d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c7f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c80` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c81` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x4c82` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5673` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5674` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5675` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5676` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5677` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5678` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5679` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x567a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x567b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x567c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x567d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x567e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x567f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5680` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5681` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5682` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5683` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5684` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5685` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5686` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5687` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x568d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x568e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x568f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58eb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58ec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58ed` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58ee` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58ef` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f2` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x58f9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5a71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5a72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5a73` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5a74` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5aa3` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5aa4` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ad5` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ad7` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ad8` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ad9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ada` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5adb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5adc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b39` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b3a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b6c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b6d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b6e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b6f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b70` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b71` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b72` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b9d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5b9e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ba0` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ba1` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5bcf` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c01` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c02` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c03` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c33` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c36` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c37` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c38` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c65` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c66` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c67` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c68` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c69` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5c6a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5cc9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5cca` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ccb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ccc` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5ccd` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5cce` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5d2d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5d2e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5d2f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5d30` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5d31` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e59` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e5a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e5b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e5c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e5d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e5e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e5f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e63` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e64` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e65` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e66` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e67` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e68` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e69` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e6a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5e6b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5f21` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5f22` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5f23` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5f24` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5f25` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5f26` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5fe9` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5fea` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5feb` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x5fec` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x604d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x6179` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x617a` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x617b` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x617c` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x617d` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x617e` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x617f` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x66a6` | — | med | auto-shape | string-table (written by ITINIT) |
| `GS 0x7d9d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7da2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7da7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dac` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dad` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dae` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7daf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7db9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dba` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dbb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dc9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dca` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dcb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dcc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dcd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dce` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dcf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dd9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dda` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ddb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ddc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ddd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dde` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ddf` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7de9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dea` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7deb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dec` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7ded` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dee` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7def` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df0` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df1` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df2` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df3` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df4` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df5` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df6` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df7` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df8` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7df9` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfa` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfb` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfc` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfd` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dfe` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7dff` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e00` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e01` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e02` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e03` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e04` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e05` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e06` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e07` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e08` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e09` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e0f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e10` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e11` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e12` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e13` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e14` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e15` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e16` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e17` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e18` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e19` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e1f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e20` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e21` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e22` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e23` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e24` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e25` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e26` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e27` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e28` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e29` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e2f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e30` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e31` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e32` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e33` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e34` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e35` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e36` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e37` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e38` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e39` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e3f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e40` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e41` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e42` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e43` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e44` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e45` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e46` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e47` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e48` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e49` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e4f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e50` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e51` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e52` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e53` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e54` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e55` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e56` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e57` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e58` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e59` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e5f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e60` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e61` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e62` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e63` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e64` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e65` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e66` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e67` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e68` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e69` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e6a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e6b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e6c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e6d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e6e` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e6f` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e70` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e71` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e72` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e73` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e74` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e75` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e76` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e77` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e78` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e79` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e7a` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e7b` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e7c` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x7e7d` | — | med | auto-shape | string-table (written by ILINIT) |
| `GS 0x8121` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8122` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8123` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8124` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8125` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8126` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8127` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8128` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8129` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x812a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8130` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8131` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8132` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8133` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8134` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8135` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8136` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8137` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8138` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8139` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x813a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x813b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x813c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x813d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x813e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x813f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8140` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8141` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8142` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8143` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8144` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8145` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8146` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8147` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8148` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8149` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x814a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x814b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x814c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x814d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x814e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x814f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8150` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8151` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8152` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8153` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8154` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8155` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8156` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8157` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8158` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8159` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x815a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x815b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x815c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x815d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x815e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x815f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8160` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8161` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8162` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8163` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8164` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8165` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8166` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8167` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8168` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8169` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x816a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x816b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8315` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8316` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8317` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8318` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8319` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x831a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x831b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x831c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x831d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x831e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x831f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8320` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8321` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8322` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8323` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8324` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8325` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8326` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8327` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8328` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8329` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x832a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x832b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x832c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x832d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x832e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8509` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x850a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x850b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x850c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x850d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x850e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x850f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8510` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8511` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8512` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8513` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8518` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8519` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x851a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x851b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x851c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x851d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x851e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x851f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8520` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8521` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x86fd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x86fe` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x86ff` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8700` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8701` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8702` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8703` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8704` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8705` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8706` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8707` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8708` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8709` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x870a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x870b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x870c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x870d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x870e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x870f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8710` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8711` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8712` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8713` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8714` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8715` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8716` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8717` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8718` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8719` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x871a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x872a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x872b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x872c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x872d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x872e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x872f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8730` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8731` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8732` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8733` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8739` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x873a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x873b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x873c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x873d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8761` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8762` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8763` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8764` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8765` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8766` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8767` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8768` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8769` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x876a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x876b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x876c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x876d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x876e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x876f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8770` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8771` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8772` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8773` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8774` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8775` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8776` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8777` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8778` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8779` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x877a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x877b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x877c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x877d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x877e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x877f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8780` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8781` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8782` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8783` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8784` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8785` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8786` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8787` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8788` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8789` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x878a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x878b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x878c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x878d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x878e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x878f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8790` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8791` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8792` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8793` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8794` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8795` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8796` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8797` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8798` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8799` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x879a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x879b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x879c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x879d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x879e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x879f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x87a0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x87a1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x87f7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x87fc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8801` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8806` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x880b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8810` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8815` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x881a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x881f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8824` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8829` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x882e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8833` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8838` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x883d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8842` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8847` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x884c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8851` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8856` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x885b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8860` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8865` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x886a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x886f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8874` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8879` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x887e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8883` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8888` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x888d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8892` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88f1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88f2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88f3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88f4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88f5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88fb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88fc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88fd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88fe` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x88ff` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8955` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8956` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8957` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x895a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x895b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x895c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8964` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8965` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8966` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8969` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x896a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x896b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x896e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x896f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8970` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8973` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8974` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8975` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8978` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8979` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x897a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8982` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8983` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8984` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8987` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8988` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8989` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x898c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x898d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x898e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8991` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8992` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8993` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8996` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8997` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8998` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x899b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x899c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x899d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89a0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89a5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89aa` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89eb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89f0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89f5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89fa` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x89ff` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8a04` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ae5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ae6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ae7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8aea` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8aeb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8aec` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8aef` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8af0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8af1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8af9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8afa` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8afb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8afe` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8aff` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b00` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b03` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b08` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b09` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b0a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b0d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b0e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b0f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b12` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b13` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b14` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b17` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b18` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b19` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b1c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b1d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b1e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b30` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b31` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b32` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b3a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b3b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b3c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b49` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b4a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b4b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b4e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b4f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b50` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b53` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b58` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b59` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8b5a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8cf7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8cfe` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d05` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d0c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d13` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d1a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d21` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d28` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d2f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d36` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d60` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d67` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d6e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d75` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d7c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d83` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d8a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d91` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d98` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8d9f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8da6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dad` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8db4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dbb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dc2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dc9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dd0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dd7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dde` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8de5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dec` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8df3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8dfa` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e01` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e08` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e0f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e16` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e1d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e24` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e2b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e32` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e39` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e40` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e47` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e4e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e55` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e5c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e63` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e6a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e71` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e78` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e7f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e86` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e8d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e94` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8e9b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ea2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ea9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8eb0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8eb7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ebe` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ec5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ecc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ed3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8eda` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ee1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ee8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8eef` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8ef6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x8efd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9aa3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9aaa` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9ab1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9ab8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9abf` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9ac6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9acd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9ad4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9adb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9ae2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9ae9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9af0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9af7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9afe` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b05` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b0c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b13` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b1a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b21` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b28` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b2f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b36` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b3d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b44` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b4b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0x9b52` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa84f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa856` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa85d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa864` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa86b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa872` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa879` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa880` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa887` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa88e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa895` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8b8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8bf` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8c6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8cd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8d4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8db` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8e2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8e9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8f0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xa8f7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb5fb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb602` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb609` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb610` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb617` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb61e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb625` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb62c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb633` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb63a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb641` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb648` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb64f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb656` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb65d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb664` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb66b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb672` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb679` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb680` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb687` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb68e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb695` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb69c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb6a3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb6aa` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb6b1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb6b8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb6bf` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb6c6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb736` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb73d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb744` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb74b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb752` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb759` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb760` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb767` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb76e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb775` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb79f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7a0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7a6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7a7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7ad` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7ae` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7b4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7b5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7bb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb7bc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8b7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8be` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8c5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8cc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8d3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8da` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8e1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8e8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8ef` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8f6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb8fd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb904` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb90b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb912` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb919` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb920` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb927` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb92e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb935` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb93c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb943` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb94a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb951` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb958` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb95f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb966` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb96d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb974` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb97b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb982` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb989` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb990` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb997` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb99e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9a5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9ac` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9b3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9ba` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9c1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9c8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9cf` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9d6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9dd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9e4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9eb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9f2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xb9f9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba00` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba07` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba0e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba15` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba1c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba23` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba2a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba31` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba38` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba3f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba46` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba4d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba54` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba5b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba62` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba69` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba70` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xba77` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbcd1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbcf4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbd17` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbd3a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbd5d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbd80` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbda3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbdc6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbde9` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbe0c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbe2f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbe52` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbe75` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbe98` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbebb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbede` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbf01` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbf24` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbf47` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbf6a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbf8d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbfb0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbfd3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xbff6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc019` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc03c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc05f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc082` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc0a5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc0c8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc0eb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc10e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3a7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3ae` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3b5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3bc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3c3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3ed` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3f4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc3fb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc402` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc409` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc663` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc66a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc671` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc686` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc68d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc694` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc6cc` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc6d3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc6da` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc6ef` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc6f6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc6fd` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc712` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc719` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc720` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc735` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc73c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc743` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc758` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc75f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc766` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc79e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7a5` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7ac` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7c1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7c8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7cf` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7e4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7eb` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc7f2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc807` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc80e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc815` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc82a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc831` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc838` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc84d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc854` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc85b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc870` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc893` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xc8b6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xca7d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xcaa0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xcac3` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xcae6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xcb09` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xcb2c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd153` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd15a` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd161` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd176` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd17d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd184` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd199` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd1a0` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd1a7` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd1df` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd1e6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd1ed` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd202` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd209` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd210` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd225` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd248` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd24f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd256` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd26b` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd272` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd279` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd28e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd295` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd29c` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd2b1` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd2b8` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd2bf` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd2d4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd2db` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd2e2` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd360` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd367` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd36e` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd3a6` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd3ad` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd3b4` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd40f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd416` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd41d` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd432` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd439` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd440` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd455` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd478` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd47f` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xd486` | — | med | auto-shape | string-table (written by SSINIT) |
| `GS 0xdef0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef3` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdef9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdefa` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf04` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf05` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf06` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf07` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf08` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf09` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf0a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf0b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf0d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf0e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf18` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf19` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf1a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf1b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf1c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf1d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf1f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf2c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf2d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf2e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf2f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf30` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf31` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf32` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf33` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf34` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf54` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf55` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf56` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf68` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf69` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf6a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf7c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf7d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdf7e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfb8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfb9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfba` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfbb` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfbc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfbd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfbf` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfc1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfcc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfcd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfce` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfcf` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd3` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfd6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfe0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfe1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfe2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfe3` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdfe5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdff4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdff5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xdff6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe01c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe01d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe01e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe01f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe020` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe021` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe030` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe031` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe032` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe033` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe044` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe045` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe046` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe047` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe058` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe059` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe05a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe05b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe05c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe06c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe06d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe06e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe094` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe0a8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe288` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe289` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe28a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe29c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe29d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe29e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2b0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2b1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2b2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2c4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2c5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2c6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2d8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2d9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2da` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2db` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2dc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2dd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2ec` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2ed` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2ee` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2ef` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2f0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe2f1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe300` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe301` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe302` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe314` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe315` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe316` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe317` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe318` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe319` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe31a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe328` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe329` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe32a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe33c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe33d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe33e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe350` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe351` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe352` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe364` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe365` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe366` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe378` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe379` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe37a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe38c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe38d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe38e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3a0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3a1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3a2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3b4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3b5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3b6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3c8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3c9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3ca` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3cb` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3cc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3cd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3dc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3dd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3de` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3f0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3f1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe3f2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe404` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe405` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe406` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe418` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe419` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe42c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe440` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe454` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6d4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6d5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6d6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6d7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6d8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6d9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6da` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6db` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6dc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6dd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6de` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6df` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e3` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6e9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6fc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6fd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6fe` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe6ff` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe700` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe701` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe702` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe703` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe704` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe705` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe706` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe707` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe708` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe709` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe70a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe70b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe70e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe70f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe710` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe711` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe724` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe725` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe726` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe727` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe728` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe729` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe72a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe72b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe72c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe72d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe72e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe72f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe732` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe733` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe74c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe74d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe74e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe74f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe750` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe751` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe752` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe753` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe754` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe755` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe756` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe757` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe758` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe759` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe75a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe75b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe75c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe75d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe7ec` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe7ed` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe7ee` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe7ef` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe7f0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe7f1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe864` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe865` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe866` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe867` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe868` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe869` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe86a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe86b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe86c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe86d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe86e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe86f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe872` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe873` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe876` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe877` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe88c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe88d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe88e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe88f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe890` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe891` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe892` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe893` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe894` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe895` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe896` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe897` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe898` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe899` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe89a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe89b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe89c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe89d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe89e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe89f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8a0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8a1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8b4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8b5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8b6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8b7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8b8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8b9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8ba` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8bb` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8be` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe8bf` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe92c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe92d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe92e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe92f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe930` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe931` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe932` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe933` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe934` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe935` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe936` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe937` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe954` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe955` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe956` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe957` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe958` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe959` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe95a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe95b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe97c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe97d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe97e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe97f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe980` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe981` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe982` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe983` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9a4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9a5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9a6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9a7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9a8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9a9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9aa` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9ab` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9ac` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xe9ad` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee04` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee05` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee06` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee07` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee08` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee09` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee2c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee2d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee2e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee2f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee30` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee31` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee54` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee55` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee56` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee57` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee58` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee59` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee7c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee7d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee7e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee7f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee80` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xee81` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeea4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeea5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeea6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeea7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeea8` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeea9` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeeaa` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeeab` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeeac` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeead` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeeae` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeeaf` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeecc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeecd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeece` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeecf` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed2` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed3` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed4` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed5` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed6` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xeed7` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef1c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef1d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef1e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef1f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef20` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef21` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef22` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef23` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef24` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef25` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef26` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef27` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef28` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xef29` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xefbc` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xefbd` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xefbe` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xefbf` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xefc0` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xefc1` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf084` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf085` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf086` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf087` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf088` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf089` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf08a` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf08b` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf08c` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf08d` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf08e` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf08f` | — | med | auto-shape | string-table (written by TIINIT) |
| `GS 0xf64d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf64e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf64f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf650` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf651` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf652` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf653` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf654` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf655` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf656` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf657` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf658` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf659` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf65a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf65b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf65c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf65d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf661` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf662` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf663` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf664` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf665` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf666` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf667` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf668` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf669` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf66a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf66b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf66c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf66d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf66e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf66f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf675` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf676` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf677` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf678` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf679` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf67a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf67b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf67c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf67d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf67e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf67f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf680` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf681` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf689` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf68a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf68b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf68c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf68d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf68e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf68f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf690` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf691` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf692` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf693` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf694` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf697` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf69b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf69c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf69d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf69e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf69f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a0` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a3` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a4` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6a6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b3` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b4` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b7` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b8` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6b9` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6ba` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6bb` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6bc` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6bd` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6c5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6c6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6c7` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6c8` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6c9` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6ca` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6cb` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6cc` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6cd` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6ce` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6cf` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6d0` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6d3` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6d7` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6d8` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6d9` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6da` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6db` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6dc` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6dd` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6de` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6df` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6e0` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6e1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6e2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf6f2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf715` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf716` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf717` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf718` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf71b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf71c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf71d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf729` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf72a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf72b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf72c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf72d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf72e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf72f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf730` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf731` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf732` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf733` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf734` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf735` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf736` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf737` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf746` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf74d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf754` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf755` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf75b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf75c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf762` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf763` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf769` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf76a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf770` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf771` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf777` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf778` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf77e` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf77f` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf785` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf786` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf78c` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf78d` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf793` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf794` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf79a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf79b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7a1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7a2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7a8` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7a9` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7af` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7b0` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7b6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7b7` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7bd` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7be` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7c4` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7c5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7cb` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7cc` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7d2` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7d3` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7d9` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7da` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7e0` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7e1` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7e7` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7e8` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7ee` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7ef` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7f5` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7f6` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7fc` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf7fd` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf803` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf804` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf80a` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf80b` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf811` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf812` | — | med | auto-shape | string-table (written by AFINIT) |
| `GS 0xf861` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf862` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf863` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf864` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf865` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf866` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf867` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf868` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf869` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf86a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf86b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf86c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf86d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf86e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf86f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf870` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf871` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf872` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf874` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf875` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf876` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf877` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf878` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf879` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf87a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf87b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf87c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf87d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf87e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf87f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf880` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf881` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf882` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf883` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf884` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf885` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf886` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf887` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf888` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf889` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf88a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf88b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf88c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf88d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf88e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf88f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf890` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf891` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf892` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf893` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf894` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf895` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf896` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf897` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf898` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf899` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf89a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf89b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf89c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf89d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf89e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf89f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a0` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a1` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a2` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a3` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a4` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a5` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a6` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a7` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a8` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8a9` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8aa` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ab` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ac` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ad` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ae` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8af` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b0` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b1` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b2` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b3` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b4` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b5` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b6` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b7` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b8` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8b9` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ba` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8bb` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8bc` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8bd` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8be` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8bf` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c0` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c1` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c2` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c3` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c4` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c5` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c6` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c7` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c8` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8c9` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ca` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8cb` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8cc` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8cd` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8ce` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8cf` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d0` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d1` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d2` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d3` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d4` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d5` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d6` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d7` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0xf8d8` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10031` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10032` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10033` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10034` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10035` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10036` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10037` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10038` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10039` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1003a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1003b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1003c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1003d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1003e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1003f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10040` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10041` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10042` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10044` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10045` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10046` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10047` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10048` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10049` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1004a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1004b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1004c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1004d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1004e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1004f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10050` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10051` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10052` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10053` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10054` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10055` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10056` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10057` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10058` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10059` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1005a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1005b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1005c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1005d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1005e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1005f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10060` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10061` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10062` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10063` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10064` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10065` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10066` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10067` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10068` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10069` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1006a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1006b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1006c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1006d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1006e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1006f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10070` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10071` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10072` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10073` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10074` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10075` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10076` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10077` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10078` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10079` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1007a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1007b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1007c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1007d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1007e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1007f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10080` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10081` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10082` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10083` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10084` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10085` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10086` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10087` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10088` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10089` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1008a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1008b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1008c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1008d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1008e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1008f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10090` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10091` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10092` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10093` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10094` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10095` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10096` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10097` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10098` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10099` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1009a` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1009b` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1009c` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1009d` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1009e` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x1009f` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a0` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a1` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a2` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a3` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a4` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a5` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a6` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a7` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x100a8` | — | med | auto-shape | string-table (written by FAINIT) |
| `GS 0x10801` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10802` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10803` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10804` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10805` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10806` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10807` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10808` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10809` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1080a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1080b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1080c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1080d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1080e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1080f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10810` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10811` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10812` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10813` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10814` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10815` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10816` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10817` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10818` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10819` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1081a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1081b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1081c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1081d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1081e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1081f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10820` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10821` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10822` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10823` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10824` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10825` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10826` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10827` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10828` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10829` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1082a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1082b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1082c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1082d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1082e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1082f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10830` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10831` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10832` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10833` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10834` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10835` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10836` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10837` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10838` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10839` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1083a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1083b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1083c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1083d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1083e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1083f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10840` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10841` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10842` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10843` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10844` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10845` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10846` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10847` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10848` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10849` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1084a` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1084b` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1084c` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1084d` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1084e` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x1084f` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10850` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10851` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10852` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10853` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x10854` | — | med | auto-shape | string-table (written by CIINIT) |
| `GS 0x108c9` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ca` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108cb` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108cc` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108cd` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ce` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108cf` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d0` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d1` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d2` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d3` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d4` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d5` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d6` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d7` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d8` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108d9` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108da` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108db` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108dc` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108dd` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108de` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108df` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e0` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e1` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e2` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e3` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e4` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e5` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e6` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e7` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e8` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108e9` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ea` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108eb` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ec` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ed` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ee` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ef` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f0` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f1` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f2` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f3` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f4` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f5` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f6` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f7` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f8` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108f9` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108fa` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108fb` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108fc` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108fd` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108fe` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x108ff` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10900` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10901` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10902` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10903` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10904` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10905` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10906` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10907` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10908` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10909` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1090a` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1090b` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1090c` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1090d` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1090e` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1090f` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10910` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10911` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10912` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10913` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10914` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10915` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10916` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10917` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10918` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10919` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1091a` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1091b` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1091c` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1091d` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1091e` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1091f` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10920` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10921` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10922` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10923` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10924` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10925` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10926` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10927` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10928` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10929` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1092a` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1092b` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1092c` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1092d` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1092e` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x1092f` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10930` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10931` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10932` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10933` | — | med | auto-shape | string-table (written by VIINIT) |
| `GS 0x10cb1` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb2` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb3` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb4` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb5` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb6` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb7` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb8` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10cb9` | — | med | auto-shape | string-table (written by SOINIT) |
| `GS 0x10ce3` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ce4` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ce5` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ce6` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ce7` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ce8` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ce9` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cea` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ceb` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cec` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10ced` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cee` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cef` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cf7` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cf8` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cf9` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10cff` | — | med | auto-shape | string-table (written by TPINIT) |
| `GS 0x10d01` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d02` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d03` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d04` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d05` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d06` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d07` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d08` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d33` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d34` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d65` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d66` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d97` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d98` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d99` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d9a` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d9b` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d9c` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10d9d` | — | med | auto-shape | string-table (written by CAINIT) |
| `GS 0x10dc9` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10dca` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10dcb` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10dd3` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10ddd` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10dde` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10ddf` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10de0` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10de1` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10de7` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10de8` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10de9` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10dea` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10deb` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10dec` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10ded` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e2d` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e2e` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e2f` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e37` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e41` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e42` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e43` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e44` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e45` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e4b` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e4c` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e4d` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e4e` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e4f` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e50` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e51` | — | med | auto-shape | string-table (written by RUINIT) |
| `GS 0x10e91` | — | med | auto-shape | string-table (written by GRINIT) |
| `GS 0x10e92` | — | med | auto-shape | string-table (written by GRINIT) |
| `GS 0x10e93` | — | med | auto-shape | string-table (written by GRINIT) |
| `GS 0x10e94` | — | med | auto-shape | string-table (written by GRINIT) |
| `GS 0x10e95` | — | med | auto-shape | string-table (written by GRINIT) |
| `GS 0x10e9b` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10e9e` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10e9f` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ea0` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ea2` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ea3` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ea4` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ea6` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ea7` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eab` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eac` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eae` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eaf` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eb2` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eb3` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eb4` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eb6` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eb7` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eba` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ebb` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ebc` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ebe` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ebf` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec0` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec2` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec3` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec4` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec6` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec7` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ec8` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ecb` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ecc` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ecd` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ece` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ecf` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed0` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed2` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed3` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed5` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed6` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed7` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed8` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ed9` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eda` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10edb` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10edd` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ede` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10edf` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee0` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee1` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee2` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee3` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee5` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee6` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee7` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee8` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ee9` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eea` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eeb` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eed` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eee` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eef` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef1` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef2` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef3` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef5` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef6` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef7` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10ef9` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10efa` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10efb` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10efc` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10efd` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10efe` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10eff` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f01` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f02` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f03` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f05` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f06` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f07` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f09` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f0a` | — | med | auto-shape | string-table (written by EX1INIT) |
| `GS 0x10f0b` | — | med | auto-shape | string-table (written by EX1INIT) |

## ui-toggle

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x3` | config_initialized | high | investigation | A shared-profile cell: SYSTEM4@0x1ac runs INITCONFIG while it is 0 and then sets it; otherwise LOADCONFIG reads the options back. |
| `0x915` | adv_message_window_shown | high | investigation | Whether the message window is shown: the window transitions set it to 1 to draw the window, then to the state they leave (1 to show, 0 to hide); draw_message_window draws only while it is set. |
| `0x917` | adv_chrome_enabled | high | investigation | Whether the message window's buttons (skip, history, auto, hide) are registered and drawn: register_adv_hotspots and draw_message_window test it; a scene's setup sets it to 1. |
| `0x30a0` | voice_group_mutes | med | investigation | An option per voice group, cleared by INITCONFIG and kept in the shared profile. A scene reads [the speaker's voice group, G[0x268f74 + G[0x268b8c + G[0x3489]]]] and runs mute-next-voice when it is set (SC0000@0xbdf). |
| `0x348a` | adv_page_has_text | high | investigation | 1 once a page has text: every dialogue page sets it first, clear_message_text clears it, and publish_message_text publishes the layout only while it and the window are shown. |
| `0x3634` | map_frame_rate | med | investigation | An option, 60 by INITCONFIG, which CONFIG toggles between 60 and 30. FIELD@0x29b waits 1000 / value ms a frame and DRAWWALK@0x182 steps 60 / value, so it is the map's frame rate. |
| `0x36e5` | on_dungeon_map | med | investigation | FIELD@0xc5 sets it on entering a dungeon map; ALLMAP and TOWN clear it. INIT2@0x1e5 walks the RMINIT chain on a load where it is set. |
| `0x36e6` | scene_over_map | med | investigation | A scene is playing over the dungeon map: SCJUMP's context 9 rules and the MPEV scripts set it. SYSTEM4 clears it after the scene and runs FADEMAPFRAME; INIT2 rebuilds the map's picture on a load where it is set. |
| `0x118ece` | in_inn | med | investigation | INN@0xe sets it on entry and INN@0x1630 clears it; INIT2@0x24e walks the QUINIT chain on a load where it is set. |
| `0x268b1a` | boot_driver_done | med | investigation | INIT2 sets 1 last (INIT2@0x269), above the saved prefix, so it is set after every boot and load; FIELD@0x655 and the MP scripts test it. |

## unknown

| address | name | conf | source | usage |
|---|---|---|---|---|
| `0x363d` | party_mode | low | investigation | Switches INIT, INN and CHECKCONTRACT between one party (0: the party in play, current_party_index) and every party (bit 20 and bits 17 to 19 of a quest's state). Read by SCJUMP's context 6 rules. Its meaning is not decoded. |
| `0x2662d5` | field_music | med | investigation | Saved: the field music playing, copied from RMINIT's map_block_music on every block (FIELD@0x6fd); FIELD fades it out when a block has none (FIELD@0x699). |
| `0x266370` | fishing_started | med | investigation | Saved: set once EXTRA1 has set up the fishing points and rod for the first time (EXTRA1@0x0030f). |
| `0x36fa00` | actor_actions | high | investigation | The acting unit's six actions, copied from EBINIT's G[0x41c22f] (RTN@0x9d); slot 0 falls back to G[0xc1ab9a] or 9999 (素手). |
| `0x3790cf` | unit_unused_0x3790cf | low | investigation | EBINIT: 1 for 400 units, 2 for the 54 party units. No script reads it. |
| `0xa26f6b` | routine_step_result | high | investigation | Set by a rule script that acts: 1 acted, 2 stood (BTL@0x31f1 turns 2 into the defend stance; RTN_M011 sets 2 on reaching its cell); 0 lets RTN fall back to RTN000 (RTN@0x283). |
| `0xd6` | — | med | auto-shape | record-table[stride 5] |
| `0x4f0` | — | med | auto-shape | record-table[stride 5] |
| `0x232a` | — | med | auto-shape | record-table[stride 5] |
| `0x34a6` | — | med | auto-shape | record-table[stride 3] |
| `0x363b` | — | low | auto-shape | index/counter? |
| `0x36d4` | — | med | auto-shape | current-entity-index? |
| `0x50183` | — | med | auto-shape | record-table[stride 200] |
| `0x80ec3` | — | med | auto-shape | record-table[stride 40] |
| `0x8ab03` | — | med | auto-shape | record-table[stride 100] |
| `0xa31a3` | — | med | auto-shape | record-table[stride 100] |
| `0xbb843` | — | med | auto-shape | record-table[stride 100] |
| `0xd3ee3` | — | med | auto-shape | record-table[stride 100] |
| `0xec583` | — | med | auto-shape | record-table[stride 100] |
| `0x104c23` | — | med | auto-shape | record-table[stride 5] |
| `0x105fab` | — | med | auto-shape | record-table[stride 5] |
| `0x107333` | — | med | auto-shape | record-table[stride 5] |
| `0x1086bb` | — | med | auto-shape | record-table[stride 5] |
| `0x10c1b7` | — | med | auto-shape | record-table[stride 8] |
| `0x118ed0` | — | med | auto-shape | record-table[stride 5] |
| `0x118edf` | — | med | auto-shape | record-table[stride 5] |
| `0x118eee` | — | med | auto-shape | record-table[stride 2] |
| `0x118f0c` | — | low | auto-shape | index/counter? |
| `0x118f10` | — | med | auto-shape | record-table[stride 2] |
| `0x11903e` | — | med | auto-shape | record-table[stride 2] |
| `0x11cfea` | — | med | auto-shape | record-table[stride 60] |
| `0x11e75a` | — | med | auto-shape | record-table[stride 60] |
| `0x11feca` | — | med | auto-shape | record-table[stride 60] |
| `0x1218f7` | — | med | auto-shape | current-entity-index? |
| `0x12195a` | — | med | auto-shape | record-table[stride 600] |
| `0x1303ba` | — | med | auto-shape | record-table[stride 600] |
| `0x13ee1a` | — | med | auto-shape | record-table[stride 900] |
| `0x154daa` | — | med | auto-shape | record-table[stride 900] |
| `0x16ad3a` | — | med | auto-shape | record-table[stride 6] |
| `0x16b7cb` | — | med | auto-shape | record-table[stride 10000] |
| `0x262da5` | — | med | auto-shape | record-table[stride 20] |
| `0x262e09` | — | med | auto-shape | record-table[stride 20] |
| `0x262f35` | — | low | auto-shape | index/counter? |
| `0x263116` | — | med | auto-shape | record-table[stride 5] |
| `0x2631ac` | — | med | auto-shape | record-table[stride 10] |
| `0x2632d8` | — | med | auto-shape | record-table[stride 5] |
| `0x264660` | — | med | auto-shape | record-table[stride 5] |
| `0x26623f` | — | med | auto-shape | record-table[stride 5] |
| `0x266385` | — | med | auto-shape | record-table[stride 4] |
| `0x268b3a` | — | low | auto-shape | index/counter? |
| `0x275683` | — | low | auto-shape | index/counter? |
| `0x275684` | — | low | auto-shape | index/counter? |
| `0x2756d4` | — | med | auto-shape | record-table[stride 60] |
| `0x276e55` | — | low | auto-shape | index/counter? |
| `0x276f00` | — | med | auto-shape | record-table[stride 600] |
| `0x27b7a8` | — | med | auto-shape | record-table[stride 10000] |
| `0x527e29` | — | med | auto-shape | record-table[stride 10] |
| `0x5404c9` | — | med | auto-shape | record-table[stride 10] |
| `0x5898a9` | — | med | auto-shape | record-table[stride 10] |
| `0x5db5d1` | — | med | auto-shape | record-table[stride 30] |
| `0x5db8bf` | — | med | auto-shape | record-table[stride 30] |
| `0x5dbbad` | — | med | auto-shape | record-table[stride 30] |
| `0x5dda5b` | — | low | auto-shape | index/counter? |
| `0x5dda75` | — | low | auto-shape | index/counter? |
| `0x5dddc1` | — | med | auto-shape | record-table[stride 100] |
| `0x5e2be1` | — | med | auto-shape | record-table[stride 100] |
| `0x5eccd1` | — | med | auto-shape | record-table[stride 5] |
| `0x5ecec5` | — | med | auto-shape | record-table[stride 5] |
| `0x84d4b9` | — | med | auto-shape | record-table[stride 5] |
| `0xaa108c` | — | low | auto-shape | index/counter? |
| `0xc0b9f5` | — | med | auto-shape | record-table[stride 5] |
| `0xc0bc11` | — | med | auto-shape | record-table[stride 600] |
| `0xc1a8c9` | — | low | auto-shape | index/counter? |
| `0xc1adb6` | — | med | auto-shape | record-table[stride 60] |
| `0xc1b4be` | — | med | auto-shape | record-table[stride 60] |
| `0xc1bbc6` | — | med | auto-shape | record-table[stride 60] |
| `0xc1ca4e` | — | med | auto-shape | record-table[stride 600] |
| `0xc2109e` | — | med | auto-shape | record-table[stride 600] |
| `0xc25984` | — | med | auto-shape | record-table[stride 4] |
| `0xc4abea` | — | low | auto-shape | index/counter? |
| `0xc4abeb` | — | med | auto-shape | record-table[stride 18] |
| `0xc4ac9f` | — | med | auto-shape | record-table[stride 18] |
| `0xc4ad53` | — | med | auto-shape | record-table[stride 18] |
| `0xc4ae07` | — | med | auto-shape | record-table[stride 18] |
| `0xc4aebb` | — | med | auto-shape | record-table[stride 18] |
| `0xc4af6f` | — | med | auto-shape | record-table[stride 18] |
| `0xc4b023` | — | med | auto-shape | record-table[stride 18] |
| `0xc4b0eb` | — | med | auto-shape | record-table[stride 2] |
| `0xc4b0ff` | — | low | auto-shape | index/counter? |
| `0xc4b100` | — | med | auto-shape | current-entity-index? |
| `0xc4b104` | — | med | auto-shape | current-entity-index? |
| `0xc4b106` | — | low | auto-shape | index/counter? |
| `0xc4b107` | — | low | auto-shape | index/counter? |
| `0xc4b10d` | — | low | auto-shape | index/counter? |
| `0xc4b112` | — | med | auto-shape | record-table[stride 2] |
