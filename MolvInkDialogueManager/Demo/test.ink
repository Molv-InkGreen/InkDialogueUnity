VAR Haogan=0

这是一个简单的演示
首先是角色A的说话！#CHAR:A:Happy 
改变tag让角色A换个表情#CHAR:A:CloseEye
tag 改变位置，移到左边#CHAR:A:Happy #POS:0
tag 改变位置，移到中间#CHAR:A:Happy #POS:1
接下来是选项
 * 选项1
 * 选项2
 -合流选项
 然后演示角色之间的对话
-角色A发起对话#CHAR:A:CloseEye
更改tag让角色B回复#CHAR:B:Happy  #POS:0
角色A说话#CHAR:A:Happy 
然后是好感演示
 * 好感+1
 ~Haogan++
 * 好感-1
 ~Haogan--
 -打印好感 {Haogan}

结束游戏

-> END


 

- They lived happily ever after.
    -> END
