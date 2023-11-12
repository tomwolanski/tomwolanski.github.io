console.info('go top');


(() => {
    const top = document.getElementById('content-start')
    const isMobile = window.matchMedia("(max-width: 48em)").matches;

    console.info(top);

    if (top && isMobile && !window.location.hash)
    {
        top.scrollIntoView({behavior: "smooth", block: "start", inline: "nearest"});
    }
})();