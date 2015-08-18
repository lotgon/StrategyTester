(* ::Package:: *)

BeginPackage["Forex`"];

RandomGauss[average_, sigma_]:= Module[{x, y, result},
	If[sigma <= 0, Throw["sigma must be more than 0"]];
	x = Random[Real, {0, 1}];
	While[0 == x, x = Random[Real, 0, 1];];
	y = Random[Real, {0, 2 \[Pi]}];
	result = average + sigma * Log[x] * Cos[y];
	result
];
Options[PortfolioByProfit] = {ConfidenceLevel -> 0.95};
PortfolioByProfit[profit_, loss_, options___]:= Module[{x, y, n, matrix, level, fprofit, factor, \[Alpha], \[CapitalSigma], system, solution, result},
	level = ConfidenceLevel/.Flatten[{options}]/.Options[PortfolioByProfit];
	If[level <= 0.5 ||level >= 1, Throw["ConfidenceLevel must be more than 0.5 and less than 1"]];
	y=y/.FindRoot[1+Erf[y/Sqrt[2]]== 2 level,{y,0}];

	n = Dimensions[profit][[1]];
	matrix = Table[N[Covariance[profit[[i]], profit[[j]] ] ],{i,n},{j,n}];
	factor = Sqrt[Max[matrix]];
	
	fprofit = Table[profit[[i]] / factor,{i,n}];
	\[Alpha] = Sum[Subscript[x, i] N[Mean[fprofit[[i]]]], {i, n}];
	matrix = Table[N[Covariance[fprofit[[i]], fprofit[[j]] ] ],{i,n},{j,n}];
	\[CapitalSigma] = Sum[y^2 Subscript[x, i] Subscript[x, j] matrix[[i,j]],{i,n}, {j,n}];
	system = {\[Alpha],\[CapitalSigma]<= 4 loss \[Alpha] / factor};
	solution = NMaximize[system, Table[{Subscript[x, i], -1, 1}, {i,n}]];
	result = {solution[[1]] * factor, Table[Subscript[x, i]/.solution[[2]], {i,n}]};
	result
];
Options[PortfolioByEquity] = {ConfidenceLevel -> 0.95};
PortfolioByEquity[equity_, loss_, options___]:= Module[{n, profit, level, result},	
	n = Dimensions[equity][[1]];
	level = ConfidenceLevel/.Flatten[{options}]/.Options[PortfolioByEquity];
	profit = Table[Differences[equity[[i]]], {i, n}];
	result = PortfolioByProfit[profit, loss, ConfidenceLevel-> level];
	result
];
Slice[data_, column_, count_]  := Module[{result, n, i, j},
	n = Dimensions[data][[1]];
	result = Table[data[[i, j]], {i, n}, {j, column, column + count}];
	result
];

EndPackage[];
