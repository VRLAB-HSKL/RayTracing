%LaTeX class file for GI AR/VR Workshop 2005
%Defines documentclass <VRARWorkshop>

% ------------ identification --------------
\NeedsTeXFormat{LaTeX2e}
\ProvidesClass{VRARWorkshop}[2005/05/02 RWTH Aachen University]
% ------------ initial code --------------
\newif\ifVRAR@german

% ------------ declaration of options --------------
\DeclareOption{titlepage}{%
    \OptionNotUsed%
    \ClassError{VRARWorkshop}{titlepage option not allowed for this layout!}{}
}
\DeclareOption{twosided}{%
    \OptionNotUsed%
    \ClassError{VRARWorkshop}{twosided option not allowed for this layout!}{}
}
\DeclareOption{german}{%
    \VRAR@germantrue
}


%forward all unknown options to the base article class
\DeclareOption*{\PassOptionsToClass{\CurrentOption}{article}}
% ------------ execution of options --------------
\ProcessOptions

% ------------ package loading --------------
\LoadClass[12pt,twoside,a4paper]{article}
\RequirePackage{a4}         % Standard is "letter"..we want DIN A4
\RequirePackage{epsfig}     % Include Encapsulated PostScript in LaTeX documents.
\RequirePackage{color}      % Color-package
\RequirePackage{latexsym}   % Now we can use different math. symbols

\ifVRAR@german
    %include german packets for "intuitive" german writing
    \RequirePackage[latin2]{inputenc}
    \RequirePackage{german}     
    \RequirePackage{t1enc}      % Now we can use german letters in the "\hyphenation" - command
    \RequirePackage{bibgerm}
\fi

% ------------ main code --------------
%Startup
\AtBeginDocument{\maketitle}


% LAYOUT OPTIONS
%base page style
\pagestyle{empty}

%offsets
\setlength{\voffset}{-1in}
\setlength{\hoffset}{-1in}

%printer driver automatically adds 1" = 25.4 mm ...
\setlength{\oddsidemargin}{25mm}
\setlength{\evensidemargin}{20mm}
\setlength{\topmargin}{20mm}

\setlength{\headheight}{0mm}
\setlength{\headsep}{0mm}
\setlength{\topskip}{0mm}

\setlength{\textwidth}{165mm}
\setlength{\textheight}{247mm}

% space between rows
\renewcommand{\baselinestretch}{1.2}

% redefine font sizes for caption/headline styles
\renewcommand{\LARGE}{
    \fontsize{16}{20.0}\selectfont
}
\renewcommand{\Large}{
    \fontsize{14}{17.5}\selectfont
}
\renewcommand{\large}{
    \fontsize{12}{14.5}\selectfont
}
%************
%NEW COMMANDS
%************
%redefine \maketitle to include title, author information, abstract and keywords
\renewcommand{\maketitle}{
    \begin{center}
        \begin{LARGE}
            \textbf{\VRAR@Title}\\[16pt]
        \end{LARGE}
        \VRAR@Authors \\[12pt]
        \VRAR@Affiliations \\[18pt]
    \end{center}
    \textbf{\ifVRAR@german Zusammenfassung: \else Abstract: \fi} \VRAR@Abstract \\[12pt]
    \textbf{\ifVRAR@german Stichworte: \else Keywords: \fi} \VRAR@Keywords \\[-16pt]
}
%tmp variables and command definitions for title layout
\newcommand{\VRAR@Title}{\ClassError{VRARWorkshop}{Please use the \\title command to define a paper title!}}
\renewcommand{\title}[1]{
    \renewcommand{\VRAR@Title}{#1}
}
\newcommand{\VRAR@Abstract}{\ClassError{VRARWorkshop}{Please use the \\abstract command to define an abstract section!}}
\renewcommand{\abstract}[1]{
    \renewcommand{\VRAR@Abstract}{#1}
}
\newcommand{\VRAR@Keywords}{\ClassError{VRARWorkshop}{Please use the \\keywords command to give some keywords!}}
\newcommand{\keywords}[1]{
    \renewcommand{\VRAR@Keywords}{#1}
}
\newcommand{\VRAR@Authors}{\ClassError{VRARWorkshop}{Please use the \\authors command to specify the list of authors!}}
\newcommand{\authors}[1]{
    \renewcommand{\VRAR@Authors}{#1}
}
\newcommand{\VRAR@Affiliations}{\ClassError{VRARWorkshop}{Please use the \\affiliations command to specify a table containing the authors' affiliations!}}
\newcommand{\affiliations}[1]{
    \renewcommand{\VRAR@Affiliations}{#1}
} 

%affiliation tag macro for "easy" creation of affiliation tags
\newcommand{\VRARafftag}[1]{$^#1$}

%bibstyle retargeting according to language switch
\newcommand{\VRARsetbibstyle}{
    \ifVRAR@german
        \bibliographystyle{geralpha}
    \else
        \bibliographystyle{alpha}
    \fi
} 
