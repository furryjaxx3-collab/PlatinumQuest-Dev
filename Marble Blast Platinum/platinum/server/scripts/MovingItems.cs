// https://github.com/Elletra/dso-sharp thanks for this super awesome tool! :3

datablock StaticShapeData(CrateItem)
{
	category = "Crates";
	className = "Crate";
	shapeFile = "~/data/shapes/crates/CrateBig.dts";
};
datablock StaticShapeData(SmallCrateItem : CrateItem)
{
	shapeFile = "~/data/shapes/crates/Crate.dts";
};
function Crate::onAdd(%this, %obj)
{
	%obj.oldTransform = %obj.position SPC %obj.rotation;
	%this.schedule(500, "makeGroup", %obj);
}

function Crate::makeGroup(%this, %obj)
{
	if (!isObject(CrateCleanup))
	{
		new SimGroup(CrateCleanup);
		if (!isObject(MissionCleanup))
		{
			new SimGroup(MissionCleanup);
			ServerGroup.add(MissionCleanup);
		}
		MissionCleanup.add(CrateCleanup);
	}
	CrateCleanup.add(%obj);
}

package CrateOverrides
{
	function onFrameAdvance(%delta)
	{
		Parent::onFrameAdvance(%delta);
		for (%i = 0; %i < $CrateCount; %i++)
		{
			if ($CrateList[%i] && LocalClientConnection.Player)
			{
				$CrateList[%i].getDataBlock().checkCollision($CrateList[%i], LocalClientConnection.Player);
			}
		}
		if (LocalClientConnection.Player)
		{
			$OldMarblePos = LocalClientConnection.Player.getPosition();
		}
	}

};
function Crate::onMissionReset(%this)
{
	resetCrates();
}

function resetCrates()
{
	for (%i = 0; %i < CrateCleanup.getCount(); %i++)
	{
		%obj = CrateCleanup.getObject(%i);
		if (%obj.getDataBlock().className $= "Crate")
		{
			%obj.setTransform(%obj.oldTransform);
		}
	}
}

function resetCrateList()
{
	for (%i = 0; %i < $CrateCount; %i++)
	{
		$CrateList[%i] = 0;
	}
	$CrateCount = 0;
}

function addToCrateList(%grp)
{
	for (%i = 0; %i < %grp.getCount(); %i++)
	{
		%obj = %grp.getObject(%i);
		if (isObject(%obj))
		{
		}
		else
		{
			if (%obj.getDataBlock().className $= "Crate")
			{
				$CrateList[$CrateCount] = %obj;
				$CrateCount++;
			}
			if (%obj.getClassName() $= "SimGroup")
			{
				addToCrateList(%obj);
			}
		}
	}
}

function Crate::checkCollision(%this, %obj, %col)
{
	%m = %obj.getWorldBoxCenter();
	%t = %col.getWorldBoxCenter();
	%d = mAbs(getWord(%t, 0) - getWord(%m, 0)) SPC mAbs(getWord(%t, 1) - getWord(%m, 1)) SPC mAbs(getWord(%t, 2) - getWord(%m, 2));
	%x = getWord(%d, 0);
	if (getWord(%d, 1) > %x)
	{
		%x = getWord(%d, 1);
	}
	if (getWord(%d, 2) > %x)
	{
		%x = getWord(%d, 2);
	}
	if (mAbs(%x) < 0.53)
	{
		%this.onCollision(%obj, %col);
	}
}

function Crate::onCollision(%this, %obj, %col)
{
	if (!%obj || !%col || !%this)
	{
		return;
	}
	%m = %obj.getWorldBoxCenter();
	%t = %col.getWorldBoxCenter();
	%d = VectorSub(%t, %m);
	%sX = getWord(%d, 0);
	%sY = getWord(%d, 1);
	%sZ = getWord(%d, 2);
	%xGreater = mAbs(%sX) > mAbs(%sY);
	%zGreater = mAbs(%sZ) > mAbs(%sX) && mAbs(%sZ) > mAbs(%sY);
	if (%zGreater)
	{
		return;
	}
	%speed = 0.3;
	if (%xGreater)
	{
		%origPos = VectorAdd(%obj.getTransform(), %speed * (%sX > 0 ? -1 : 1) SPC "0 0 0 0 0 0");
	}
	else
	{
		%origPos = VectorAdd(%obj.getTransform(), "0" SPC %speed * (%sY > 0 ? -1 : 1) SPC "0 0 0 0 0");
	}
	if (isObject(%obj))
	{
		%obj.setTransform(%origPos);
	}
}

function Crate::getReset(%this)
{
	$OldMarblePos = LocalClientConnection.Player.getPosition();
}