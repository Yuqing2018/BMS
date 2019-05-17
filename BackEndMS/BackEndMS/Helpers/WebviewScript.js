var result = '';
AddASButton();
var b_results = document.getElementById("b_results");
if (null != b_results) {
    var urlAllBtn = document.createElement('BUTTON');
    var urlAllText = document.createTextNode('SubmitAllUrl');
    urlAllBtn.appendChild(urlAllText);
    urlAllBtn.addEventListener('click', addAllUrlText);
    b_results.insertBefore(urlAllBtn, b_results.childNodes[0]);
}
var b_algos = document.getElementsByClassName('b_algo');
for (var i = 0; i < b_algos.length; i++) {
    insertButton(b_algos[i]);
}
var b_context = document.getElementById('b_context');
if (b_context != null) {
    var ul_b_vList = b_context.getElementsByClassName("b_vList")[0];
    //insertRelatedButton(ul_b_vList);//RelatedPanel 插入submitAll按钮
    if (ul_b_vList != null) {
        var x = document.createElement('BUTTON');
        var t = document.createTextNode('SubmitAll');
        x.appendChild(t);
        x.addEventListener('click', addAllrelatedText);
        ul_b_vList.insertBefore(x, ul_b_vList.childNodes[0]);
    }
    var b_vLists = b_context.querySelectorAll('.b_vList li');
    for (var i = 0; i < b_vLists.length; i++) {
        insertRelatedButton(b_vLists[i]);
        var a = b_vLists[i].querySelector('a');
        if (null != a)
            a.addEventListener("click", RefreshWebViewControl);
    }
}

AddImageSearchRS();

function AddASButton() {
    var searchBar = document.getElementById("sb_form");
    searchBar.querySelector('#sb_form_q').addEventListener("focus", AddASEvent);
    searchBar.addEventListener("submit", SubmitAS);
}
function AddASEvent() {
    var list = document.querySelectorAll("li.sa_sg");
    list.forEach(item => {
        item.addEventListener("click", function () {
            if (null != this.url) {
                window.external.notify(this.url + ";" + this.query);
            }
        });
    });
}
function SubmitAS() {
    var searchValue = this.q.defaultValue;
    if (searchValue != "") {
      //  window.external.notify(searchValue);
        if (this.action == "https://www.bing.com/images/search")
            window.external.notify(this.action + "?q=" + encodeURI(searchValue) +";"+ searchValue);
        else {
            var list = this.querySelectorAll("li.sa_sg");
            var KeywordsAS = [];
            list.forEach(item => {
                KeywordsAS.push(item.query);
            });
            var allASInfo = searchValue + ";3;" + KeywordsAS.join(';');
            window.external.notify(allASInfo);
        }
    }
    
}
function addAllUrlText() {
    var b_algoLists = this.parentElement.querySelectorAll('li.b_algo');
    for (var i = 0; i < b_algoLists.length; i++) {
        b_algoLists[i].getElementsByTagName('button')[0].click();
    }
}
function addAllrelatedText() {
    var b_vLists = this.parentElement.querySelectorAll('.b_vList li');
    for (var i = 0; i < b_vLists.length; i++) {
        b_vLists[i].getElementsByTagName('button')[0].click();
        //insertRelatedButton(b_vLists[i]);
    }
}
function insertButton(obj) {
    var x = document.createElement('BUTTON');
    var t = document.createTextNode('Submit');
    x.appendChild(t);
    x.addEventListener('click', changeText);
    obj.insertBefore(x, obj.childNodes[0]);
}
function changeText() {
    var btnUrl = document.getElementById('sb_form_q').value + ';' + this.parentElement.querySelector('h2 a').href + ";1";
    //result += btnUrl; 
    //document.getElementById('urlPan').innerText = result;
    window.external.notify(btnUrl);
}
function insertRelatedButton(obj) {
    var x = document.createElement('BUTTON');
    var t = document.createTextNode('Submit');
    x.appendChild(t);
    x.addEventListener('click', addrelatedText);
    obj.appendChild(x);
}
function addrelatedText() {
    var btnUrl = document.getElementById('sb_form_q').value + ';' + this.previousElementSibling.innerText + ";2";
    //result += btnUrl; 
    //document.getElementById('urlPan').innerText = result;
    window.external.notify(btnUrl);
}

function AddImageSearchRS() {
    var ol = document.querySelectorAll(".carousel-content .items");//document.getElementsByClassName("items");
    if (null != ol && ol.length > 0) {
        for (var i = 0; i < ol.length; i++) {
            var imageRsAllBtn = document.createElement('BUTTON');
            var imageRsAllText = document.createTextNode('SubmitAllImageAs');
            imageRsAllBtn.appendChild(imageRsAllText);
            imageRsAllBtn.addEventListener('click', addAllimageRsText);
            ol[i].insertBefore(imageRsAllBtn, ol[i].childNodes[0]);
        }
    }
    var ol_List = document.querySelectorAll("li.item.col");
    //document.getElementsByClassName("item col");
    for (var i = 0; i < ol_List.length; i++) {
        insertImageSearchRSButton(ol_List[i]);
        var a = ol_List[i].querySelector("a");
        if (null != a)
            a.addEventListener("click", RefreshWebViewControl);

    }
}
function RefreshWebViewControl(obj) {
    if (null != this.href) {
        window.external.notify(this.href + ";" + this.innerText.replace(/\n/g, ""));
    }
}
function insertImageSearchRSButton(obj) {
    var div = document.createElement("div");
    var x = document.createElement('BUTTON');
    var t = document.createTextNode('Submit');
    x.appendChild(t);
    x.addEventListener('click', addImageRSText);
    div.appendChild(x);
    obj.insertBefore(div, obj.childNodes[0]);
}
function addImageRSText(obj) {
    //var RsUrl = this.parentElement.getElementsByClassName("suggestion-item ")[0].href;
   // var RsContent = this.parentElement.nextElementSibling.getElementsByClassName('suggestion-title')[0].innerText;
    //var btnUrl = document.getElementById('sb_form_q').value + ';' + RsContent + ";1";
    //var RsUrl = this.parentElement.querySelector("a.suggestion-item ").href;
    //var RsContent = this.parentElement.querySelector('span.suggestion-title').innerText;
    //var btnUrl = document.getElementById('sb_form_q').value + ';' + RsContent + ";1;" + RsUrl;
    //var Rsele_a = this.parentElement.querySelector("a.suggestion-item ");//Rsele_a.href
    var PQsContent = this.parentElement.nextElementSibling.querySelector("span.suggestion-title");
    var flag = 4;
    if (null == PQsContent) {
        PQsContent = this.parentElement.nextElementSibling.querySelector("div.cardInfo");
        flag = 5;
    }
    var RsContent = null == PQsContent ? "" : PQsContent.innerText.replace(/\n/g, "");
    var btnUrl = document.getElementById('sb_form_q').value + ";" + RsContent + ";"+flag;
    window.external.notify(btnUrl);
}
function addAllimageRsText(obj) {
    var ol_List = this.parentElement.querySelectorAll('li.item.col');
    for (var i = 0; i < ol_List.length; i++) {
        ol_List[i].getElementsByTagName('button')[0].click();
    }
}
    