﻿/********************************************************************
**       This head file is generated by program,                   **
**            Please do not change it directly.                    **
********************************************************************/

#ifndef CSV_CONFIG_HEAD_H_H_HHHH
#define CSV_CONFIG_HEAD_H_H_HHHH
#include "CSVConfigStruct.h"
#include <map>
using namespace std;

class CHeroQualityUpTest {
    public:
    CHeroQualityUpTest() { };
    ~CHeroQualityUpTest() { };

    bool Load(const std::string & szFileName);
    void Clear() { m_mapVipLevelTestTemplate.clear(); }

    public:
    const SHeroQualityUpTest* FindContent( int  id) const;

    map<int, SHeroQualityUpTest> m_mapHeroQualityUpTest;
};
class CVipLevelTest {
    public:
    CVipLevelTest() { };
    ~CVipLevelTest() { };

    bool Load(const std::string & szFileName);
    void Clear() { m_mapVipLevelTestTemplate.clear(); }

    public:
    const SVipLevelTest* FindContent( int  id) const;

    map<int, SVipLevelTest> m_mapVipLevelTest;
};



#endif /* CSV_CONFIG_HEAD_H_H_HHHH */