using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalWordCounter
{
    partial class CWC_Server
    {
    //====================================================================================================================================
    // *** CWC SERVER FRONTEND GENERATION FUNCTIONS *** //
    //====================================================================================================================================
    //
    //
        //===============================================================================================================
        /// <summary>
        /// Generates the HTML content for the homepage, displaying a list of available text files with clickable links,
        /// styled as paper from a notebook.
        /// </summary>
        /// <returns>The HTML content of the homepage.</returns>
        //===============================================================================================================
        private string GenerateHomepage()
        {
            StringBuilder responseBody = new StringBuilder();
            responseBody.AppendLine(@"<html>
                                        <head>
                                            <title>CapitalWordCounter</title>
                                            <style>
                                                :root {
                                                    --margin-line: #941c5a;
                                                    --lines: #1d97b8;
                                                }

                                                html,
                                                body {
                                                    background-color: #eee;
                                                    display: flex;
                                                    align-items: center;
                                                    justify-content: center;
                                                    margin: 0;
                                                    min-height: 100%;
                                                    font-family: 'Brush Script MT', cursive;
                                                }

                                                .paper {
                                                    width: 850px;
                                                    background-color: #fff;
                                                    background-image: linear-gradient(var(--lines) 0.05em, transparent 0.05em);
                                                    background-size: 100% 2em;
                                                    position: relative;
                                                    box-shadow: 15px 15px 33px rgba(27, 27, 27, 0.1);
                                                }

                                                .paper:before,
                                                .paper:after {
                                                    content: '';
                                                    position: absolute;
                                                    top: 0;
                                                }

                                                .paper:before {
                                                    height: 100%;
                                                    width: 2px;
                                                    background-color: var(--margin-line);
                                                    left: 4em;
                                                    z-index: 2;
                                                }

                                                .paper:after {
                                                    height: 6em;
                                                    width: 100%;
                                                    background-color: #fff;
                                                    left: 0;
                                                    z-index: 1;
                                                }

                                                .holes {
                                                    background-color: #eee;
                                                    width: 35px;
                                                    height: 35px;
                                                    border-radius: 50%;
                                                    display: block;
                                                    z-index: 10;
                                                    margin-left: 1em;
                                                    margin-top: 6em;
                                                }

                                                .holes:before,
                                                .holes:after {
                                                    content: '';
                                                    width: 35px;
                                                    height: 35px;
                                                    background-color: #eee;
                                                    position: absolute;
                                                    border-radius: 50%;
                                                }

                                                .holes:before {
                                                    top: 50%;
                                                }

                                                .holes:after {
                                                    top: calc(100% - 400em);
                                                }

                                                h1 {
                                                    color: #941c5a;
                                                    text-align: center;
                                                    margin-bottom: 30px;
                                                }

                                                a {
                                                    display: inline-block;
                                                    color: #1d97b8;
                                                    text-decoration: none;
                                                    font-weight: bold;
                                                    margin-left: 1em;
                                                    padding-left: 74px;
                                                    font-size: 20px;
                                                    line-height: 2em;
                                                    height: 1.7em;
                                                    max-width: calc(100% - 8em);
                                                }

                                                a:hover {
                                                    text-decoration: underline;
                                                    padding-left: 80px;
                                                }
                                            </style>
                                            <link rel=""icon"" href=""data:,"">
                                        </head>
                                        <body>
                                            <div class='paper'>
                                                <div class='holes'></div>
                                                    <h1>Available Text Files:</h1>");


            string[] files = Directory.GetFiles(RootFolder, "*.txt", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                responseBody.AppendLine($"<a href='{fileName}'>{fileName}</a><br>");
            }


            responseBody.AppendLine(@"</div> </body> </html>");

            return responseBody.ToString();
        }



        //==========================================================================================================================
        /// <summary>
        /// Generates an HTML response containing the specified file name and its corresponding word count, styled as a paper note.
        /// </summary>
        /// <param name="fileName">The name of the file to display in the response.</param>
        /// <param name="wordCount">The count of words, with more than five letters, to display in the response.</param>
        /// <returns>The HTML response content.</returns>
        //==========================================================================================================================
        private string GenerateResponse(string fileName, string wordCount)
        {
            StringBuilder response = new StringBuilder();

            response.AppendLine(@"<html>
                            <head>
                                <title>CapitalWordCounter</title>
                                <style>
                                    :root {
                                        --paper-shadow: #c9bf8d;
                                    }

                                    body {
                                        display: flex;
                                        justify-content: center;
                                        padding: 10vmin;
                                        background-color: #eee;
                                        font-family: 'Caveat', cursive;
                                        font-size: 2rem;
                                    }

                                    .paper {
                                        position: relative;
                                        --paper-dark: #e5c93d;
                                        --paper-color: #ffed87;
                                        position: relative;
                                        display: flex;
                                        justify-content: center;
                                        width: 325px; /* Fixed width */
                                        height: 270px; /* Fixed height */
                                        background: linear-gradient(135deg, var(--paper-dark), 30%, var(--paper-color));
                                        box-shadow: 3px 3px 2px var(--paper-shadow);
                                        transform: rotate(10deg);
                                        transform-origin: top left;
                                        flex-direction: column;
                                        align-items: center;
                                    }

                                    .paper p {
                                        margin: auto;
                                        margin: 5px 0;
                                        display: block;
                                        text-align: center;
                                    }

                                    .pin {
                                        --pin-color: #d02627;
                                        --pin-dark: #9e0608;
                                        --pin-light: #fc7e7d;
                                        position: absolute;
                                        left: 20px;
                                        top: -10px;
                                        width: 60px;
                                        height: 50px;
                                    }

                                    .shadow {
                                        position: absolute;
                                        top: 18px;
                                        left: -8px;
                                        width: 35px;
                                        height: 35px;
                                        border-radius: 50%;
                                        background: radial-gradient(var(--paper-shadow), 20%, rgba(201, 191, 141, 0));
                                    }

                                    .metal {
                                        position: absolute;
                                        width: 5px;
                                        height: 20px;
                                        background: linear-gradient(to right, #808080, 40%, #eae8e8, 50%, #808080);
                                        border-radius: 0 0 30% 30%;
                                        transform: rotate(50deg);
                                        transform-origin: bottom left;
                                        top: 15px;
                                        border-bottom: 1px solid #808080;
                                    }

                                    .bottom-circle {
                                        position: absolute;
                                        right: 15px;
                                        width: 35px;
                                        height: 35px;
                                        border-radius: 50%;
                                        background-color: var(--pin-color);
                                        background: radial-gradient(
                                            circle at bottom right,
                                            var(--pin-light),
                                            25%,
                                            var(--pin-dark),
                                            90%,
                                            var(--pin-color)
                                        );
                                    }

                                    /* Barrel */
                                    .bottom-circle::before {
                                        content: '';
                                        position: absolute;
                                        top: 0;
                                        left: -2px;
                                        width: 20px;
                                        height: 30px;
                                        transform: rotate(55deg);
                                        transform-origin: 100% 100%;
                                        border-radius: 0 0 40% 40%;
                                        background: linear-gradient(
                                            to right,
                                            var(--pin-dark),
                                            30%,
                                            var(--pin-color),
                                            90%,
                                            var(--pin-light)
                                        );
                                    }

                                    /* Top circle */
                                    .bottom-circle::after {
                                        content: '';
                                        position: absolute;
                                        right: -10px;
                                        top: -5px;
                                        width: 25px;
                                        height: 25px;
                                        border-radius: 50%;
                                        background: radial-gradient(
                                            circle at right,
                                            var(--pin-light),
                                            30%,
                                            var(--pin-color),
                                            var(--pin-dark) 80%
                                        );
                                    }
                                </style>
                                <link rel=""icon"" href=""data:,"">
                            </head>
                            <body>
                                <div class='paper'>
                                    <div class='pin'>
                                        <div class='shadow'></div>
                                        <div class='metal'></div>
                                        <div class='bottom-circle'></div>
                                    </div>
                                    <p style=""font-size: 24px;"">" + fileName + @"</p>
                            <p style=""font-size: 34px;""><b>Capital words:</b></p>
                            <p style=""font-size: 14px; margin-top: 0px; margin-bottom: 15px;"" > (with more than 5 letters)</p>
                            <p><b>" + wordCount + @"</b></p>
                                </div>
                            </body>
                        </html>");

            return response.ToString();
        }
    //
    //
    //====================================================================================================================================
    // *** END *** //
    //====================================================================================================================================

    }
}
