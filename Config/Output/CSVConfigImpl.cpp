﻿/********************************************************************
**       This head file is generated by program,                   **
**            Please do not change it directly.                    **
********************************************************************/

#include "CSVConfigHead.h"

#if defined(WIN32) || defined(WIN64)
#else
#include <stdio.h>
#include <unistd.h>
#include <dirent.h>
#include <stdlib.h>
#include <sys/stat.h>
#include <string.h>
#include <assert.h>
#endif

bool CHeroQualityUpTest::Load(const std::string & szFileName)
{
	try
	{
		csv::CSVParser oParser = csv::CSVParser(szFileName);
		for (int i = 3; i < oParser.RowCount(); ++i) {
			SHeroQualityUpTest conent;
		    int nColumn = 1;
		    int id = atoi(oParser.GetField(i, nColumn++).c_str());
		    conent.id = id;
		    int llKey = id;
			conent.id = oParser.ReadInt32(i, nColumn++);
			conent.hero_id = oParser.ReadInt32(i, nColumn++);
			conent.quality = oParser.ReadInt32(i, nColumn++);
			conent.gold = oParser.ReadInt32(i, nColumn++);
			conent.maxHP = oParser.ReadInt32(i, nColumn++);
			conent.attack = oParser.ReadFloat(i, nColumn++);
			conent.defense = oParser.ReadFloat(i, nColumn++);
			conent.date = oParser.ReadString(i, nColumn++);
			conent.starttime = oParser.ReadString(i, nColumn++);

		    m_mapHeroQualityUpTest[llKey] = conent;
		}
	}
	catch (const std::exception&)
	{
		return false;
	}
	
	return true;
}
const SHeroQualityUpTest *CHeroQualityUpTest::FindContent(int	id) const
{
	int llKey = id;

	auto it = m_mapHeroQualityUpTest.find(llKey);
	if (it != m_mapHeroQualityUpTest.end())
	{
		return &it->second;
	}
	return NULL;
}
bool CVipLevelTest::Load(const std::string & szFileName)
{
	try
	{
		csv::CSVParser oParser = csv::CSVParser(szFileName);
		for (int i = 3; i < oParser.RowCount(); ++i) {
			SVipLevelTest conent;
		    int nColumn = 1;
		    int id = atoi(oParser.GetField(i, nColumn++).c_str());
		    conent.id = id;
		    int llKey = id;
			conent.id = oParser.ReadInt32(i, nColumn++);
			conent.vip_level = oParser.ReadInt32(i, nColumn++);
			conent.vip_exp = oParser.ReadInt32(i, nColumn++);
			conent.sweeper = oParser.ReadInt32(i, nColumn++);
			conent.purchase_energy_count = oParser.ReadInt32(i, nColumn++);
			conent.purchase_midas_count = oParser.ReadInt32(i, nColumn++);
			conent.reset_elite_count = oParser.ReadInt32(i, nColumn++);
			conent.reset_arena_count = oParser.ReadInt32(i, nColumn++);
			conent.reset_expedition_count = oParser.ReadInt32(i, nColumn++);
			conent.signpatch = oParser.ReadInt32(i, nColumn++);
			conent.unlock_function = oParser.ReadString(i, nColumn++);
			conent.unlock_fuction_att = oParser.ReadString(i, nColumn++);
			conent.skillpoint_restore_time = oParser.ReadInt32(i, nColumn++);
			conent.midas_crit_rate = oParser.ReadFloat(i, nColumn++);
			conent.add_arena_count = oParser.ReadInt32(i, nColumn++);
			conent.arena_cd = oParser.ReadInt32(i, nColumn++);
			conent.item5 = oParser.ReadString(i, nColumn++);
			conent.item6 = oParser.ReadString(i, nColumn++);
			conent.equivalent_value = oParser.ReadInt32(i, nColumn++);
			conent.purchase_huoli_count = oParser.ReadInt32(i, nColumn++);
			conent.purchase_cristal_count = oParser.ReadInt32(i, nColumn++);
			conent.cristal_crit_rate = oParser.ReadFloat(i, nColumn++);
			conent.reset_dungeon_count = oParser.ReadInt32(i, nColumn++);

		    m_mapVipLevelTest[llKey] = conent;
		}
	}
	catch (const std::exception&)
	{
		return false;
	}
	
	return true;
}
const SVipLevelTest *CVipLevelTest::FindContent(int	id) const
{
	int llKey = id;

	auto it = m_mapVipLevelTest.find(llKey);
	if (it != m_mapVipLevelTest.end())
	{
		return &it->second;
	}
	return NULL;
}

