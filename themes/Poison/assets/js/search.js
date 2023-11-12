const fuseOptions = {
  shouldSort: true,
  includeMatches: true,
  threshold: 0.0,
  tokenize:true,
  ignoreLocation: true,
  distance: 100,
  maxPatternLength: 32,
  minMatchCharLength: 1,
  keys: [
    {name:"title",weight:0.8},
    {name:"contents",weight:0.5},
    {name:"tags",weight:0.3},
    {name:"categories",weight:0.3}
  ]
}

const setupSearch = function() {
    fetch('../index.json')
    .then(resp => resp.json())
    .then(data => {
      const fuse = new Fuse(data, fuseOptions)

      const input = document.getElementById('searchInput')
      const results = document.getElementById('searchResults')

      input.addEventListener('keyup', (e) => {
        const items = fuse.search(e.target.value)

        if (items.length > 0) {
          let itemList = ''

          itemList  += `<ul class="entries">`

          for (let i in items) {

            itemList += `<li>`
                    +  `   <span class="title"> <a href="${items[i].item.permalink}">${items[i].item.title}</a> </span>`
                    +  `   <span class="published"> <time class="pull-right post-list">${items[i].item.date}</time> </span>`
                    +  `</li>`
          }

          itemList += `</ul>`

          results.innerHTML = itemList
        } else {
          results.innerHTML = ''
        }
      });

      // Prevent search clears
      input.addEventListener('keydown', (e) => {
        if (e.key === "Enter") {
          e.preventDefault()
        }
      });

    });
}
