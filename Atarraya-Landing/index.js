var data = { email: "" };

var button = document.getElementById("submit");

function sendData() {

    data = { email: String(document.getElementById("emailInput").value) };

    fetch("/api/submitEmail/", {
        method: "POST",
        body: JSON.stringify(data)
    }).then(res => {
        alert(String(data.email) + " ha sido registrado exitosamente!\nPronto recibiras noticias de nosotros!");
    }, rej => {
        alert(rej.body);
    });
}

