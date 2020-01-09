DialogueTree圖形化對話編輯器
=====

## 前言
在和同學製作的畢業專題遊戲《沉沒意志》參展版本完成後，我對開發中碰到的編輯龐大對話系統的繁雜問題進行審視，也基於自我能力提升，而研究製作此一圖形化對話編輯器。\
[沉沒意志介紹與下載連結](https://kdkd44223.wixsite.com/website/blank-1)



## 介紹
遊戲對話系統的圖形化編輯器\
可用樹狀節點的結構來編輯有多條分歧路線的遊戲劇情對話\
編輯器在工具列的目錄為Window/Dialogue Tree\
![image](https://github.com/StupidBute/DialogueTree/blob/master/DialogueTree0.jpg)
上圖的檔案此專案檔中的位址為Assets/Resources/DemoStory，可用編輯器左下角的開啟按鈕開啟。


左上角的面板可以新增所有會參與對話的角色，方便之後的節點取用。每個角色除了可以更改名稱外，也可更改作為標籤用的顏色，讓各個節點所屬的腳色可以一目了然。
![image](https://github.com/StupidBute/DialogueTree/blob/master/DialogueTree1.JPG)


中間的網格狀主畫面可以按右鍵新增各種節點，包含劇情節點、對話集節點、問答集節點以及分歧節點。
![image](https://github.com/StupidBute/DialogueTree/blob/cbfd62fd8a1365ef55c303fac495b46091054d94/DialogueTree2.jpg)

* 劇情節點：
>沒有實質功能，而是作為一段劇情對話的開頭識別所用。後續的各式節點依照對話順序連接到劇情節點上，遊戲運行時就可以透過遊戲腳本呼叫此劇情節點來依序索引對話。

* 對話集節點：
>最普遍使用的節點，用來編寫大部分不需要分支的對話。除對話內容以外，也可在此設定對話所使用的字體大小、要搭配的動作編號，或是要程式同步執行的指令，運用範圍很廣。

* 問答集節點：
>當NPC要向玩家提問時所使用的節點，節點內可編輯NPC的問話以及玩家的應答。玩家應答的數量會直接對應到問答子節點，再由子節點連接到該應答之後的對話。

* 分歧節點：
>此節點內不包含實際的對話內容，而是單純作為判斷節點的功能使用。與問答集相似，每個分歧路線都對應到一個分歧子節點，可再由這些子節點連結到該路線後續的對話。一個分歧路線可以設立多條條件，而目前條件的種類分為兩種——劇情開關與玩家應答。劇情開關可以由任何地方觸發並記錄在對話控制腳本中，而可以因應各種條件的判斷。玩家應答則是可以往前追蹤玩家與任一位NPC的任一問答所選擇的選項來進行判斷。


編輯好節點後，對節點點擊右鍵即可創造連結。從劇情節點開始依序將各個對話與分歧路線串連起來後，就可以用編輯器左下角的按鈕儲存。節點的資訊將被轉換成scriptable_story的腳本化物件。同時也可以用左下角的開啟按鈕來在編輯器中開啟已編輯好的腳本化物件。



## 製作中功能
由於此編輯器是針對我的畢業專題遊戲所設計，因此不少功能還未完全從此專題的腳本中獨立出來，很多類別依然是需要互相調用的。目前正在嘗試將遊戲腳本與編輯器腳本兩者的函式與類別做切割，讓彼此都能獨立運作。
