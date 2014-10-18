using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nfklib.NMap
{

    /// <summary>
    /// Special objects helper
    /// Use next link as a manual https://github.com/HarpyWar/nfkmap-viewer/wiki/Специальные-объекты-на-карте
    /// </summary>
    public static class SpecialObject
    {
        public static TMapObj Teleport(short x, short y, short goto_x, short goto_y)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 1;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.length = goto_x;
		    obj.dir = goto_y;

		    return obj;
	    }

	    public static TMapObj Button(short x, short y, short color, short wait, short target, short shootable)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 2;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.orient = color;
		    obj.wait = wait;
		    obj.target = target;
		    obj.special = shootable;

		    return obj;
	    }

	    public static TMapObj Door(short x, short y, short orient, short length, short wait, short targetname, short fastclose)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 3;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
            obj.orient = orient;
		    obj.length = length;		
		    obj.wait = wait;
		    obj.targetname = targetname;
		    obj.special = fastclose;

		    return obj;
	    }

	    public static TMapObj Trigger(short x, short y, short length_x, short length_y, short wait, short target)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 4;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.length = length_x;
		    obj.dir = length_y;		
		    obj.wait = wait;
		    obj.target = target;

		    return obj;
	    }

	    public static TMapObj AreaPush(short x, short y, short length_x, short length_y, short wait, short target, short direction, short pushspeed)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 5;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.length = length_x;
		    obj.dir = length_y;
		    obj.wait = wait;
		    obj.target = target;
		    obj.orient = direction;
		    obj.special = pushspeed;

		    return obj;
	    }

	    public static TMapObj AreaPain(short x, short y, short length_x, short length_y, short wait, short dmginterval, short dmg)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 6;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.special = length_x;
		    obj.orient = length_y;
		    obj.wait = wait;
		    obj.nowanim = dmginterval;
		    obj.dir = dmg;

		    return obj;
	    }

	    public static TMapObj AreaTrixarenaEnd(short x, short y, short length_x, short length_y)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 7;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.special = length_x;
		    obj.orient = length_y;

		    return obj;
	    }

	    public static TMapObj AreaTeleport(short x, short y, short length_x, short length_y, short goto_x, short goto_y)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 1;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.dir = length_x;
		    obj.wait = length_y;
		    obj.special = goto_x;
		    obj.orient = goto_y;

		    return obj;
	    }

	    public static TMapObj DoorTrigger(short x, short y, short orient, short length, short target)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 1;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.orient = orient;
		    obj.length = length;
		    obj.target = target;

		    return obj;
	    }

	    public static TMapObj AreaWaterillusion(short x, short y, short length_x, short length_y)
	    {
		    var obj = new TMapObj();
		    obj.objtype = 1;
		    obj.active = 1;
		
		    obj.x = x;
		    obj.y = y;
		    obj.special = length_x;
		    obj.orient = length_y;

		    return obj;
	    }
    }
}
