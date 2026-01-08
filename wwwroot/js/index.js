let page = 0;
let pageSize = 36;
let gird = document.getElementsByClassName("event-grid")[0];
let template = Handlebars.compile(document.getElementById("events-template").innerHTML);
let isEverythingLoaded = false;
let everythingLoaded = document.getElementById("everything-loaded");

window.addEventListener("load", function (){
    AddMoreData();
    everythingLoaded.style.display = "none";
});
window.addEventListener("scroll", handleScroll);

function handleScroll() {
    if (isScrolledToBottom()){
        AddMoreData();
    }
}

function isScrolledToBottom() {
    const windowHeight = window.innerHeight;
    const documentHeight = document.body.clientHeight || document.documentElement.clientHeight;
    const scrollTop = window.scrollY || window.pageYOffset;

    // Check if the user has reached the bottom of the page
    return windowHeight + scrollTop >= documentHeight;
}

function AddMoreData(){
    //var eventMatches = LoadMoreData();    
    LoadMoreData( function (data){
        if(data == null){
            everythingLoaded.style.display = "block";
        }
        else {
            gird.innerHTML += template(data);
        }
        console.log(data);
        console.log(template(data));
    })
}


function LoadMoreData(callback) {
    $.ajax({
        type: "POST",
        url: "/Home/GetJsonEvents/",
        data: {Page: page, PageSize: pageSize},
        //dataType: "string",  
        success: function(data) {
            page += 1;
            console.log(data);
            if(callback){
                callback(data);
            }
        },
        error: function () {
            isEverythingLoaded = true;
            if(callback){
                callback("[]");
            }        
        }
    });
}

