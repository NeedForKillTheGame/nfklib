NFK Helper Library
======

Извлекает информацию из демки и из карты игры [Need For Kill](http://needforkill.ru)

В [тестовой программе](https://github.com/HarpyWar/nfklib/blob/master/test/Program.cs) показан пример вывода статистики:

![](http://i.imgur.com/Y4zmfdU.png)

Можно извлечь и сохранить карту `.mapa` из демки:
```cs
var fileName = "demo.ndm";
var ndm = new nfklib.NDemo.NFKDemo();
var demo = ndm.Read(fileName);
// палитра
demo.Map.Palette.Save("palette.png", ImageFormat.Png);
// карта
ndm.Map.Write("mapfromdemo.mapa");
```

Можно создать свою карту, или изменить существующую:
```cs
var nmap = new NFKMap();
var map = nfkmap.NewMap(15, 8);

// следующий код заполнит бриками границу карты
for (int x = 0; x < map.Header.MapSizeX; x++)
	for (int y = 0; y < map.Header.MapSizeY; y++)
		if (x == 0 || x == map.Header.MapSizeX - 1 || y == 0 || y == map.Header.MapSizeY - 1)
			map.Bricks[x][y] = 228;

// респавн в левом нижнем углу
map.Bricks[1][map.Header.MapSizeY - 2] = SimpleObject.Respawn();

// установим в правом нижнем углу портал, с телепортом в левый нижний угол
var obj = SpecialObject.Teleport
(
	(short)(map.Header.MapSizeX - 2), // x
	(short)(map.Header.MapSizeY - 2), // y
	2, // goto x
	(short)(map.Header.MapSizeY - 2) // goto y
);

map.Objects = new TMapObj[] { obj }; // добавить портал в массив объектов

nmap.Write("test.mapa");
```
![](http://i.imgur.com/eAna7FE.png)
