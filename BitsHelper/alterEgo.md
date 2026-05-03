# BitsHelper / Alter Ego

## Triggers

### AlterEgoTrigger

在节点位置产生*另一个你*, 按下 Mod 的 `Switch Between Players` 按键(默认 Tab)以切换到*另一个你*

- `visualMode`: 视觉模式
	- `SameAsPlayer`: 与玩家相同
	- `Backpack`: 携带背包
	- `NoBackpack`: 不携带背包
	- `MadelineAsBadeline`: Badeline

### AlterEgoRemoveTrigger

在场上有多个玩家时碰到的玩家会被移除, 如果被移除的是当前所控制的玩家则会切换到下一个玩家  
最开始的玩家不会被移除

- `oneUse`: 是否为一次性的
- `flag`: 移除成功后设置的 flag

### AlterEgoConfigTrigger

- `holdInteractions`: 是否允许玩家间抓取交互
- `boopInteractions`: 是否允许玩家间踩头交互

两种交互默认关闭

### AlterEgoResetTrigger

清除所有相关内容并切换至最开始的玩家

## Entities

### AlterEgoBlockField

阻挡玩家间切换的区域

- `blockIn`: 开启后位于此区域的玩家不能被切换进来
- `blockOut`: 开启后如果当前玩家位于此区域则不能切换到其他玩家

## 其他预期行为

- 假设场上只有一个玩家的 Mod 所认为的玩家将会是当前控制所的玩家
- 只有当前控制原本玩家时才可以进行切板, 切板后其他玩家会被移除

## 未定行为

- 使用其他类似的双生 Mod

---

# BitsHelper / Alter Ego

## Triggers

### AlterEgoTrigger

Spawns *another you* at the node position, press the Mod's `Switch Between Players` key (default to **Tab**) to switch to *another you*

- `visualMode`: Visual mode
	- `SameAsPlayer`: Same as player
	- `Backpack`: With backpack
	- `NoBackpack`: Without backpack
	- `MadelineAsBadeline`: Badeline

### AlterEgoRemoveTrigger

Remove the player when there are multiple players. If the removed player is the currently controlled player, it will switch to the next player.  
The original player will not be removed.

- `oneUse`: Whether it is one-time use
- `flag`: The flag to set after a successful removal

### AlterEgoConfigTrigger

- `holdInteractions`: Whether to allow grab interactions
- `boopInteractions`: Whether to allow boop interactions

These two interactions are disabled by default

### AlterEgoResetTrigger

Reset all related content and switch to the original player

## Entities

### AlterEgoBlockField

A field that blocks switching between players

- `blockIn`: When enabled, players in this area cannot be switched in
- `blockOut`: When enabled, if the current player is in this area, they cannot switch to other players

## Other Expected Behaviors

- Mod that assume there is only one player in the level will consider the currently controlled player as the player.
- Only the original player can do transition, and after transition, other players will be removed.

## Undefined Behaviors

- Use with other "doppelgnger" Mods