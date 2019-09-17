The IST-Thesis-MSC LaTeX template can be used for Instituto Superior Técnico Master of Science thesis of DEI (or other departments), as it follows the regulations published by the Scientific Council of IST.

To start using, just open and modify your data in the following files:

1. IST-Thesis-MSc-Preamble_commands.tex
Select the ‘main’ language of your thesis in the line that loads the package ‘babel’:

	\usepackage[main=english,portuguese]{babel}
	
	or 
	\usepackage[english,main=portuguese]{babel}
	
The thesis document will then automgically modify all the adequate keywords, titles, captions, etc. according to the selected language.

Later, you may modify parameters or include other packages if deemed necessary.

2. IST-Thesis-MSc-Front_Cover.tex
In this document insert your data (name, thesis titles, supervisors, date, degree, etc.):

	\title{}
	\subtitle{}
	\author{}
	\degree{}
	\supervisor{}
	\othersupervisor{}
	\date{}

For a draft thesis (before examination) modify the following command to the value ‘false’:

	\finalthesis{false}

If it is for the final version (approved after examination) then modify the command to the value ‘true’:

	\finalthesis{true}

and include all the members of the examination Committee (except the Supervisor):

	\chairperson{}
	\vogalone{}
	\vogaltwo{}
	\vogalthree{}

3. The content of your thesis will be written in the documents in folder ‘Chapters’
Do not modify the ‘header’ in all those documents, except of the name of the document as it contains compilation directives.
 
If you need to add more Chapters, just create new .tex documents and add the header.
If you do not need that many Chapters in your thesis, just comment the lines in the main document:

  	‘IST-Thesis-MSc-Main_Document.tex’

For example, if you do not need a 6th Chapter, comment the following lines:

	%Chapter 6
	\input{./Chapters/IST-Thesis-MSc-Chapter_6.tex}
	% If Printing on DOUBLE SIDED pages, the second page should be white.
	% Otherwise, comment the following command:
	\cleardoublepage

4. IST-Thesis-MSc-Bibliography.bib
This file is the Bibliography database. Just replace it with your own, and ig f the name ios different, just modifify the corresponding line in in the main document ‘IST-Thesis-MSc-Main_Document.tex’:

	\bibliography{./IST-Thesis-MSc-Bibliography}

#####################

Enjoy

Rui Santos Cruz