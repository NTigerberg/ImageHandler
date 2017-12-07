import { Component } from '@angular/core';
import { Headers, Http } from '@angular/http';
import { AnalysisResultModel } from './analysisResultModel';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(public http: Http	) {

  }

  public title = 'Image Uploader';
  public jsonResult: AnalysisResultModel;

  public message: string;

  public images: string[];

  public tags: string[];
  public url = 'http://localhost:35195/api/image/upload';

  onUploadFinished($event) {
    this.jsonResult = JSON.parse($event.serverResponse._body);
    this.message = this.jsonResult.Message;
    this.images = this.jsonResult.Images;
    this.tags = this.jsonResult.Tags;
    console.log(this.jsonResult);
  }
}
