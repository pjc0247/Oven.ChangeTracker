Oven.ChangeTracker
=====

dip tracking for non-relational storages.


__Property Tracking__
```cs
Console.WriteLine( player.HasChanges ); // false

player.Name = "KimSlim";

Console.WriteLine( player.HasChanges ); // true
player.ConfirmChanges();

Console.WriteLine( player.HasChanges ); // false
```

__Collection Tracking__
```cs
player.Items.Clear();

Console.WriteLine( player.HasChanges ); // true
```
```cs
player.Items.Add(ObservedEntity.Create<Item>());

Console.WriteLine( player.HasChanges ); // true
```

__Tracking Graph__<br>
하위 오브젝트가 바뀌면, 상위 오브젝트의 변경도 마크됨.
```cs
var sword = ObservedEntity.Create<Item>();
player.Items.Add(sword);

player.ConfirmChanges();
Console.WriteLine( player.HasChanges ); // false

sword.Grade = ItemGrade.Epic;

Console.WriteLine( player.HasChanges ); // true;
Console.WriteLine( sword.HasChanges ); // true;
```