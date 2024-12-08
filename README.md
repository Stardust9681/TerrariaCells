# TerraCells
### A Dead Cells inspired Terraria mod
Currently in alpha build stage.

### For Playtesters
Please keep in mind that nothing in this build is finalized. It's likely that the balancing may be off, some features may be annoyingly buggy, and that
the mod may become unplayable for various reasons. We hope to fix any issues as such that may arise as we figure out the early pain points of development.

Please do not create duplicate reports for bugs by checking if any have already been created. If one exists, you can instead upvote it to 
help determine where prioritization is needed.

You can report bugs [todo] <!-- TODO: Update the link after figuring out where to report bugs to -->

--- 
### For Devs
Every item needs a categorization to be picked up in the custom inventory, and categorizing it depends on where the item is defined.

If the item is defined in the mod source code (aka inherits `Terraria.Item`), or in another mod that wants to be compatible, 
then the item class should implement the [`ITerraCellsCategorization`](/Content/UI/ITerraCellsCategorization.cs) interface, 
located in the `TerrariaCells.UI` namespace (as of December 2024).

If the item is defined in the vanilla source code, it will need a manual override.
To add one, navigate to [`InventoryManager`](/Content/UI/InventoryManager.cs) @ line 78 
and add a new key-value pair corresponding to the `Terraria.ID.ItemID` and the categorization.
Please keep the list sorted by the TerraCells categorization of the item. 
You can also add a subcategorization here for if your item is categorized as `TerraCellsItemCategory.Storage`,
though the subcategorization currently serves no function outside of keeping coins as exceptionary to the current inventory system.

There is currently no way to categorize an item that doesn't have a vanilla `Terraria.ID.ItemID` and that cannot implement `ITerraCellsCategorization`.
<!-- TODO: Update once that is implemented, if it ever is. -->