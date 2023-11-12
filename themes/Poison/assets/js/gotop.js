(() => {
    const top = document.getElementById('content-start')
    const isMobile = window.matchMedia("(max-width: 48em)").matches;

    if (top && isMobile && !window.location.hash)
    {
        top.scrollIntoView(true);
        //top.scrollIntoView({behavior: "smooth", block: "start", inline: "nearest"});
    }
})();