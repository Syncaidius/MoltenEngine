let toggler = document.getElementsByClassName("namespace-toggle");
let i;

for (i = 0; i < toggler.length; i++) {{
  toggler[i].addEventListener("click", function() {{
		this.parentElement.querySelector(".sec-namespace-inner").classList.toggle("sec-active");
		this.classList.toggle("namespace-toggle-down");
	  }});
}}

let pageTargets = document.getElementsByClassName("doc-page-target");
for (i = 0; i < pageTargets.length; i++) {{
		pageTargets[i].addEventListener("click", function(e) {{
			document.getElementById('content-target').src = e.target.dataset.url
		}});
}}