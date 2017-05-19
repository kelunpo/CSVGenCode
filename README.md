### CSVGenCode
Auto generate code from CSV config, user-defined the templete code
###### 一.使用方式：
　　　1.将CSV文件拷贝到 ./Config/Input 中 格式参考下面
　　　2.双击CSVGenCode.sln 运行即可
　　　3.在 Config/Output可以查看最终生成的代码
###### 二.CSV格式
```
　　　第一行为注释
　　　第二行为生成的代码中的属性名字
　　　第三行为生成的代码中的类型
　　　剩下的4~$ 为内容
```
###### 三.目录说明
```
　　　 Config/Templet 中所有文件作为模版文件
　　　 Config/Input 中所有的csv文件作为输入
　　　 Config/Output 为生成的代码
　　　 Config/KeywordMapRule.txt 作为配置生成的代码的调整配置 具体使用可以查看该文 件内容
```　 
###### 四.修改模版
```
　　　如果生成的代码不合心意　可以修改./Config/Templet中的文件
其中可以使用的关键字如下
1.#FileName         	//文件名字            
2.#KeyTypeName      	//CSV主键类型           
3.#KeyName          	//CSV主键名字
4.#StructName       	//结构体名字
5.#ClsName          	//类名
6.#AttriName        	//CSV中属性的名字         
7.#AttriTypeName            //CSV中属性的类型
8.#AttriCommment            //CSV中属性的注释   
9.#AttriType2FuncName       //CSV中属性到方法的映射 参考Type2FuncNameMap

这些宏可以替换成最终的CSV中的内容
   1.类 宏标志(一个文件模版中可以有多个)
   #Begin_Replace_Tag_Class
   #End_Replace_Tag_Class
   在这个范围内的所有代码都将作为模版
   将遍历所有的CSV,并将每一个CSV作为一个单位进行替换
   
   2.属性 宏标志(一个类宏中可以有多个)
   #Begin_Replace_Tag_Attri
   #End_Replace_Tag_Attri
   在这个范围内的所有代码都将作为模版
   将遍历单个CSV的所有列,并将每一列作为一个单位进行替换
```
如：
```cpp
#Begin_Replace_Tag_Class
struct #StructName
{
#Begin_Replace_Tag_Attri
	//#AttriCommment
	#AttriTypeName		#AttriName;
#End_Replace_Tag_Attri
};
#End_Replace_Tag_Class
```
![csv 图片](https://github.com/JiepengTan/CSVGenCode/tree/master/Screenshots/5.png)

最终生成的代码为
```cpp
struct SHeroQualityUpTest
{
	//索引ID
	int		id;
	//英雄ID
	int		hero_id;
	//英雄品质
	int		quality;
	//升阶金钱
	int		gold;

};
struct SVipLevelTest
{
	//索引ID
	int		id;
	//VIP等级
	int		vip_level;
	//VIP升级所需经验
	int		vip_exp;
	//扫荡券
	int		sweeper;
	//可购买体力次数
	int		purchase_energy_count;
};
```
```
其他的例子可以打开参考
CSVGenCode.sln
```
 
